using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IntelliCenterControl.Droid
{
    [Service]
    public class MacroTaskService : Service
    {
        CancellationTokenSource _cts;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            //_cts = new CancellationTokenSource();

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnDestroy ()
        {
            if (_cts != null) {
                _cts.Token.ThrowIfCancellationRequested ();

                _cts.Cancel ();
            }
            base.OnDestroy ();
        }
    }
}