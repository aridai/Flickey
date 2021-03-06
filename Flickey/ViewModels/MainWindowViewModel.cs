﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flickey.ViewModels
{
    using Flickey.Models;

    /// <summary>
    /// MainWindowのViewModel層の処理を提供します。
    /// </summary>
    public sealed class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly Inputter inputter = new Inputter();

        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly ReadOnlyReactiveProperty<InputMode> imeModeProperty;

        /// <summary>
        /// Keyboardの入力を受け取るコマンド。
        /// </summary>
        public DelegateCommand<string> Command { get; }

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        public MainWindowViewModel()
        {
            //  コマンドとそのコマンドパラメータの通知をする。
            var commandSubject = new Subject<string>();
            Command = new DelegateCommand<string>(character =>
            {
                if (!string.IsNullOrWhiteSpace(character))
                    commandSubject.OnNext(character);
            });

            //  IMEの入力モードを通知する。
            imeModeProperty = Observable.Interval(TimeSpan.FromSeconds(0.015))
                .Select(_ => KeyboardOperator.GetInputMode())
                .ToReadOnlyReactiveProperty()
                .AddTo(this.disposable);

            commandSubject.Subscribe(character => this.inputter.Input(character)).AddTo(this.disposable);
        }

        /// <summary>
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}