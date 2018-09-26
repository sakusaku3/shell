using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace shell.View
{
	public class ShellConsole : RichTextBox
	{
		static ShellConsole()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(ShellConsole),
				new FrameworkPropertyMetadata(typeof(ShellConsole)));
		}

		#region ShellHost dependency property
		public ViewModel.IShellHost ShellHost
		{
			get { return (ViewModel.IShellHost)this.GetValue(ShellHostProperty); }
			set { this.SetValue(ShellHostProperty, value); }
		}

		public static readonly DependencyProperty ShellHostProperty 
			= DependencyProperty.Register(
				nameof(ShellHost), 
				typeof(ViewModel.IShellHost), 
				typeof(ShellConsole), 
				new PropertyMetadata(default(ViewModel.IShellHost), HandleShellHostChanged));

		private static void HandleShellHostChanged(
			DependencyObject p_sender, 
			DependencyPropertyChangedEventArgs p_e
			)
		{
			var l_instance = (ShellConsole)p_sender;
			var l_oldValue = (ViewModel.IShellHost)p_e.OldValue;
			var l_newValue = (ViewModel.IShellHost)p_e.NewValue;

			// 古い方のホストからイベントハンドラを削除
			if (l_oldValue != null)
			{
				((INotifyCollectionChanged)l_oldValue.Invocations).CollectionChanged -= l_instance.HandleInvocations;
			}

			// 新しい方のホストへイベントハンドラを登録
			if (l_newValue != null)
			{
				((INotifyCollectionChanged)l_newValue.Invocations).CollectionChanged += l_instance.HandleInvocations;
				var l_last = l_newValue.Invocations.LastOrDefault();
				if (l_last != null) l_instance.Next(l_last);
			}
		}
		#endregion

		#region ErrorForeground dependency property
		public Brush ErrorForeground
		{
			get { return (Brush)this.GetValue(ErrorForegroundProperty); }
			set { this.SetValue(ErrorForegroundProperty, value); }
		}

		public static readonly DependencyProperty ErrorForegroundProperty 
			= DependencyProperty.Register(
				nameof(ErrorForeground), 
				typeof(Brush), 
				typeof(ShellConsole), 
				new PropertyMetadata(default(Brush)));
		#endregion

		#region フィールド
		private InvocationSection currentSection;
		#endregion

		#region コンストラクタ
		public ShellConsole()
		{
			this.Loaded += (sender, args) => DataObject.AddPastingHandler(this, this.HandlePaste);
			this.Unloaded += (sender, args) => DataObject.RemovePastingHandler(this, this.HandlePaste);

			CommandManager.AddPreviewExecutedHandler(this, this.HandleCommandExecuted);

			this.Document.PageWidth = 2000;
			this.Document.Blocks.Remove(this.Document.Blocks.FirstBlock);
		}
		#endregion

		#region メソッド
		private void HandleInvocations(object p_sender, NotifyCollectionChangedEventArgs p_e)
		{
			if (!this.Dispatcher.CheckAccess())
			{
				this.Dispatcher.Invoke(() => this.HandleInvocations(p_sender, p_e));
				return;
			}

			if (p_e.Action == NotifyCollectionChangedAction.Add)
			{
				this.Next(p_e.NewItems.OfType<ViewModel.IShellInvocation>().First());
			}
		}

		/// <summary>
		/// 次セクション生成
		/// </summary>
		/// <param name="p_invocation"></param>
		private void Next(ViewModel.IShellInvocation p_invocation)
		{
			var l_section = new InvocationSection(this, p_invocation);

			this.Document.Blocks.Add(l_section);

			if (l_section.CanEditing)
			{
				this.currentSection = l_section;
				this.CaretPosition = l_section.Editor.ContentEnd;
			}
		}

		private void Invoke()
		{
			if (this.currentSection == null
				|| this.currentSection.Invocation.Status != Constants.InvocationStatus.Ready)
			{
				return;
			}

			var l_area = this.currentSection.Editor;
			var l_prompt = this.currentSection.Prompt;
			var l_script = new TextRange(l_area.ContentStart.GetPositionAtOffset(l_prompt.Length + 1), l_area.ContentEnd).Text;

			this.currentSection.Invocation.Script = l_script;
			this.currentSection.Invocation.Invoke();
		}

		private void HandleCommandExecuted(object p_sender, ExecutedRoutedEventArgs p_e)
		{
			if (!this.CaretIsInEditArea())
			{
				if (p_e.Command == ApplicationCommands.Cut ||
					p_e.Command == ApplicationCommands.Delete)
				{
					p_e.Handled = true;
				}
			}
		}

		private void MoveCaretToDocumentEnd()
		{
			if (!this.CaretIsInEditArea())
			{
				this.CaretPosition = this.Document.ContentEnd;
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs p_e)
		{
			base.OnPreviewKeyDown(p_e);

			if (p_e.Key == Key.Enter)
			{
				this.Invoke();
				p_e.Handled = true;
			}
			else if (this.CaretIsInEditArea())
			{
				if (p_e.Key == Key.Escape)
				{
					this.currentSection.Invocation.Script = "";
					p_e.Handled = true;
				}
				if (p_e.Key == Key.Up)
				{
					this.currentSection.Invocation.SetNextHistory();
					p_e.Handled = true;
				}
				else if (p_e.Key == Key.Down)
				{
					this.currentSection.Invocation.SetPreviousHistory();
					p_e.Handled = true;
				}
				else if ((p_e.Key == Key.Left || p_e.Key == Key.Back)
					&& this.CaretIsInLeftMostOfEditArea())
				{
					p_e.Handled = true;
				}
			}
			else
			{
				if (p_e.Key == Key.Back || p_e.Key == Key.Delete)
				{
					p_e.Handled = true;
				}
				else if (Interop.GetCharFromKey(p_e.Key) != null)
				{
					this.MoveCaretToDocumentEnd();
				}
			}
		}

		private void HandlePaste(object p_sender, DataObjectPastingEventArgs p_e)
		{
			var l_isText = p_e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
			if (!l_isText) return;

			var l_text = p_e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
			if (l_text == null) return;

			this.MoveCaretToDocumentEnd();
		}

		private bool CaretIsInEditArea()
			=> this.currentSection != null && this.currentSection.CaretIsInEditArea(this.CaretPosition);

		private bool CaretIsInLeftMostOfEditArea()
			=> this.currentSection != null && this.currentSection.CaretIsInLeftMostOfEditArea(this.CaretPosition);
		#endregion

		#region インナークラス
		/// <summary>
		/// 相互運用
		/// </summary>
		private static class Interop
		{
			private enum MapType : uint
			{
				// ReSharper disable InconsistentNaming
				// ReSharper disable UnusedMember.Local
				MAPVK_VK_TO_VSC = 0x0,
				MAPVK_VSC_TO_VK = 0x1,
				MAPVK_VK_TO_CHAR = 0x2,
				MAPVK_VSC_TO_VK_EX = 0x3,
				// ReSharper restore UnusedMember.Local
				// ReSharper restore InconsistentNaming
			}

			[DllImport("user32.dll")]
			private static extern int ToUnicode(
				uint wVirtKey,
				uint wScanCode,
				byte[] lpKeyState,
				[Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
				int cchBuff,
				uint wFlags);

			[DllImport("user32.dll")]
			private static extern bool GetKeyboardState(byte[] lpKeyState);

			[DllImport("user32.dll")]
			private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

			/// <summary>
			/// 入力されたキーの文字を返し、文字でない場合は null を返します。
			/// </summary>
			public static char? GetCharFromKey(Key p_key)
			{
				char? l_ch = null;

				var l_virtualKey = KeyInterop.VirtualKeyFromKey(p_key);
				var l_keyboardState = new byte[256];
				Interop.GetKeyboardState(l_keyboardState);

				var l_scanCode 
					= Interop.MapVirtualKey(
						(uint)l_virtualKey, 
						MapType.MAPVK_VK_TO_VSC);

				var l_stringBuilder = new StringBuilder(2);

				var l_result 
					= Interop.ToUnicode(
						(uint)l_virtualKey, 
						l_scanCode, 
						l_keyboardState, 
						l_stringBuilder, 
						l_stringBuilder.Capacity,
						0);

				switch (l_result)
				{
					case -1:
						break;
					case 0:
						break;
					case 1:
						l_ch = l_stringBuilder[0];
						break;
					default:
						l_ch = l_stringBuilder[0];
						break;
				}

				return l_ch;
			}
		}

		/// <summary>
		/// 呼び出しセクション
		/// </summary>
		public class InvocationSection : Section
		{
			#region フィールド
			/// <summary>
			/// オーナーコンソール
			/// </summary>
			private readonly ShellConsole owner;
			#endregion

			#region プロパティ
			/// <summary>
			/// PowerShell呼び出し
			/// </summary>
			public ViewModel.IShellInvocation Invocation { get; }

			public bool CanEditing { get; private set; }

			public Paragraph Editor { get; }

			public string Prompt { get; }
			#endregion

			#region コンストラクタ
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="p_owner">オーナーコンソール</param>
			/// <param name="p_invocation">呼び出しオブジェクト</param>
			public InvocationSection(ShellConsole p_owner, ViewModel.IShellInvocation p_invocation)
			{
				if (p_invocation.Result == null)
				{
					this.owner = p_owner;
					this.Invocation = p_invocation;
					this.Invocation.PropertyChanged += this.HandleInvocationPropertyChanged;

					this.Prompt = $"[{p_invocation.Number}] > ";
					this.Editor = new Paragraph();
					this.Editor.Inlines.Add(this.Prompt);
					this.Blocks.Add(this.Editor);

					this.CanEditing = p_invocation.Status == Constants.InvocationStatus.Ready;
				}
				else
				{
					this.owner = p_owner;
					this.Invocation = p_invocation;
					this.SetResult(p_invocation.Result);

					this.CanEditing = false;
				}
			}
			#endregion

			private void HandleInvocationPropertyChanged(object p_sender, PropertyChangedEventArgs p_e)
			{
				if (!this.Dispatcher.CheckAccess())
				{
					this.Dispatcher.Invoke(() => this.HandleInvocationPropertyChanged(p_sender, p_e));
					return;
				}

				switch (p_e.PropertyName)
				{
					case nameof(ViewModel.IShellInvocation.Script):
						{
							if (Equals(this, this.owner.currentSection))
							{
								var range = new TextRange(this.Editor.ContentStart.GetPositionAtOffset(this.Prompt.Length + 1), this.Editor.ContentEnd);
								var script = this.Invocation.Script;
								range.Text = script;
								this.owner.CaretPosition = this.Editor.ContentEnd;
							}
							break;
						}
					case nameof(ViewModel.IShellInvocation.Status):
						{
							switch (this.Invocation.Status)
							{
								case Constants.InvocationStatus.Ready:
									this.CanEditing = true;
									break;

								case Constants.InvocationStatus.Invoking:
									this.CanEditing = false;
									break;

								case Constants.InvocationStatus.Invoked:
									this.CanEditing = false;
									this.Invocation.PropertyChanged -= this.HandleInvocationPropertyChanged;
									break;
							}
							break;
						}
					case nameof(ViewModel.IShellInvocation.Result):
						{
							this.SetResult(this.Invocation.Result);
							break;
						}
				}
			}

			private void SetResult(ViewModel.InvocationResult result)
			{
				switch (result.Kind)
				{
					case Constants.InvocationResultKind.Empty:
						break;

					case Constants.InvocationResultKind.Normal:
						{
							var paragraph = new Paragraph();
							paragraph.Inlines.Add(result.Message);
							this.Blocks.Add(paragraph);
							break;
						}

					case Constants.InvocationResultKind.Error:
						{
							var paragraph = new Paragraph();
							paragraph.Inlines.Add(result.Message);
							paragraph.Foreground = this.owner.ErrorForeground;
							this.Blocks.Add(paragraph);
							this.Blocks.Add(new Paragraph());
							break;
						}
				}
			}

			public bool CaretIsInEditArea(TextPointer p_caretPostion)
				=> this.CanEditing
					&& this.Editor.ContentStart.IsInSameDocument(p_caretPostion)
					&& this.Editor.ContentStart.GetOffsetToPosition(p_caretPostion) - 1 >= this.Prompt.Length;

			public bool CaretIsInLeftMostOfEditArea(TextPointer p_caretPostion)
				=> this.CanEditing
					&& this.Editor.ContentStart.IsInSameDocument(p_caretPostion)
					&& this.Editor.ContentStart.GetOffsetToPosition(p_caretPostion) - 1 == this.Prompt.Length;
		}
		#endregion
	}
}