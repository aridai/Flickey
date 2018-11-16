using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flickey.Controls
{
    /// <summary>
    /// Interaction logic for Keyboard.xaml
    /// </summary>
    public partial class Keyboard : UserControl, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        //  5x5=25個のキーを保持しておく。
        private readonly Key[][] keys = new Key[5][];

        //  25個のキーすべてのTouch系のイベントをまとめて通知する。
        private readonly IObservable<(Key key, TouchEventArgs args)> touchDownStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchUpStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchMoveStream;

        //  入力操作中かどうかを通知・保持する。
        private readonly ReadOnlyReactiveProperty<bool> isInputOperationInProgress;

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Keyboard()
        {
            InitializeComponent();

            //  25個のキーを配列に格納する。
            for (int y = 0; y < 5; y++)
            {
                this.keys[y] = new Key[5];
                for (int x = 0; x < 5; x++)
                {
                    var name = $"Key{y}{x}";
                    var key = (Key)this.FindName(name);
                    this.keys[y][x] = key.AddTo(this.disposable);
                }
            }

            //  すべてのイベントをまとめておく。
            //  それらをHot変換しておく。
            this.touchDownStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchDownAsObservable.Select(args => (key, args)))
                .Merge()
                .Publish();

            this.touchUpStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchUpAsObservable.Select(args => (key, args)))
                .Merge()
                .Publish();

            this.touchMoveStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchMoveAsObservable.Select(args => (key, args)))
                .Merge()
                .Publish();

            //  手動でConnectを呼んでおく。
            //  破棄はDispose時にまとめてやる。
            (this.touchDownStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);
            (this.touchUpStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);
            (this.touchMoveStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);

            //this.touchDownStream.Subscribe(tuple => System.Diagnostics.Debug.WriteLine($"Down")).AddTo(this.disposable);
            //this.touchUpStream.Subscribe(tuple => System.Diagnostics.Debug.WriteLine($"Up")).AddTo(this.disposable);
            //this.touchMoveStream.Subscribe(tuple => System.Diagnostics.Debug.WriteLine($"Move")).AddTo(this.disposable);
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