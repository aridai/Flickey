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
        //  TouchMoveイベントが消失してから、入力操作の終了判定までのタイムアウト。
        private readonly TimeSpan moveEventTimeout = TimeSpan.FromSeconds(0.3);

        //  入力操作時のスライド入力とみなされる指の移動量のしきい値。
        private readonly float distanceThreshold = 64.0f;

        private readonly CompositeDisposable disposable = new CompositeDisposable();

        //  5x5=25個のキーを保持しておく。
        private readonly Key[][] keys = new Key[5][];

        //  25個のキーすべてのTouch系のイベントをまとめて通知する。
        private readonly IObservable<(Key key, TouchEventArgs args)> touchDownStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchUpStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchMoveStream;

        //  入力操作中かどうかを通知・保持する。
        private readonly ReadOnlyReactiveProperty<bool> isInputOperationInProgress;

        //  入力操作時の相対的な指の位置を通知する。
        //  具体的な座標ではなく、上下左右などで表される。
        private readonly ReadOnlyReactiveProperty<RelativeFingerPos> fingerPos;

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

            //  入力操作中かどうかを通知する。
            //  タイムアウト時間やTouchDownイベントのみしか流れなかった場合については要検証。
            //  (たまにTouchUpイベントとTouchMoveイベントが流れないことがある。)
            this.isInputOperationInProgress = Observable.Merge(
                this.touchDownStream.Select(_ => true),
                this.touchUpStream.Select(_ => false),
                this.touchMoveStream.Throttle(this.moveEventTimeout)
                .ObserveOnDispatcher().Select(_ => false))
                .ToReadOnlyReactiveProperty()
                .AddTo(this.disposable);

            //  指の移動量を通知する。
            //  複数箇所で使うためHot変換しておく。
            var fingerMovementStream = Observable.WithLatestFrom(
                this.touchMoveStream.Select(tuple => tuple.args.GetTouchPoint(null).Position),
                this.touchDownStream.Select(tuple => tuple.args.GetTouchPoint(null).Position),
                (current, starting) => current - starting).Publish();
            fingerMovementStream.Connect().AddTo(this.disposable);

            //  指の位置を通知する。
            this.fingerPos = fingerMovementStream
                .Select(vec =>
                {
                    if (Math.Abs(vec.X) > this.distanceThreshold || Math.Abs(vec.Y) > this.distanceThreshold)
                    {
                        if (Math.Abs(vec.X) > Math.Abs(vec.Y)) return (vec.X > 0) ? RelativeFingerPos.Right : RelativeFingerPos.Left;
                        else return (vec.Y > 0) ? RelativeFingerPos.Bottom : RelativeFingerPos.Top;
                    }

                    return RelativeFingerPos.Neutral;
                }).ToReadOnlyReactiveProperty().AddTo(this.disposable);
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