using System;
using Prism.Commands;
using Prism.Mvvm;

namespace Flickey.ViewModels
{
    public sealed class MainWindowViewModel : BindableBase
    {
        public DelegateCommand<string> Command { get; }

        public MainWindowViewModel()
        {


            Command = new DelegateCommand<string>(character =>
            {
                System.Diagnostics.Debug.WriteLine($"入力:{character}");

                //Models.KeyboardOperator.InputDirectly("おっぱい");

                //  スキャンコードを設定すると死ぬやつがあるっぽい?
                //Models.KeyboardOperator.InputKey(Models.VirtualKeyCode.Kanji);

                //Models.KeyboardOperator.InputKey(new Models.VirtualKeyCode[] { Models.VirtualKeyCode.Shift, Models.VirtualKeyCode.A });

                //Models.KeyboardOperator.SetInputMode(Models.InputMode.JapaneseInput);
            });
        }
    }
}