﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.Devices.Bluetooth;
using Native = Windows.Devices.Bluetooth.GenericAttributeProfile.GattDeviceService;


namespace Plugin.BluetoothLE
{
    public class GattService : AbstractGattService
    {
        readonly DeviceContext context;
        readonly Native native;


        public GattService(DeviceContext context, Native native) : base(context.Device, native.Uuid, false)
        {
            this.context = context;
            this.native = native;
        }


        IObservable<IGattCharacteristic> characteristicOb;
        public override IObservable<IGattCharacteristic> WhenCharacteristicDiscovered()
        {
            this.characteristicOb = this.characteristicOb ?? Observable.Create<IGattCharacteristic>(async ob =>
            {
                var result = await this.native.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                foreach (var characteristic in result.Characteristics)
                {
                    var wrap = new GattCharacteristic(this.context, characteristic, this);
                    ob.OnNext(wrap);
                }
                return Disposable.Empty;
            })
            .Replay()
            .RefCount();

            return this.characteristicOb;
        }
    }
}
