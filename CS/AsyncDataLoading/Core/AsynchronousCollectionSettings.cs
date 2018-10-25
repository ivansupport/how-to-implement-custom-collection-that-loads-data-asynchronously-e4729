using System;
namespace AsyncDataLoading {
    public static class AsynchronousCollectionSettingsFactory {
        public static AsynchronousCollectionSettings CreateOnDemandRequestDataModeSettings() {
            return new AsynchronousCollectionSettings();
        }
        public static AsynchronousCollectionSettings CreateOnDemandRequestDataModeSettings(int requestDataRate) {
            return new AsynchronousCollectionSettings(requestDataRate);
        }

        public static AsynchronousCollection2Settings CreateInBackgroundThreadRequestDataModeSettings() {
            return new AsynchronousCollection2Settings();
        }
        public static AsynchronousCollectionSettings CreateInBackgroundThreadRequestDataModeSettings(int requestDataRate) {
            return new AsynchronousCollection2Settings(requestDataRate);
        }
        public static AsynchronousCollection2Settings CreateInBackgroundThreadRequestDataModeSettings(int requestDataRate, int backgroundRequestDataRate, TimeSpan backgroundRequestDataInterval) {
            return new AsynchronousCollection2Settings(requestDataRate, backgroundRequestDataRate, backgroundRequestDataInterval);
        }
    }
    public class AsynchronousCollectionSettings {
        public readonly static AsynchronousCollectionSettings Default = new AsynchronousCollectionSettings();
        public int RequestDataRate { get; set; }
        public virtual RequestDataMode Mode { get { return RequestDataMode.OnDemand; } }
        public AsynchronousCollectionSettings() {
            RequestDataRate = 40;
        }
        public AsynchronousCollectionSettings(int requestDataRate) {
            RequestDataRate = requestDataRate;
        }

        protected internal virtual void Apply<T>(AsynchronousCollection<T> col) {
            col.RequestDataRate = RequestDataRate;
        }
    }
    public class AsynchronousCollection2Settings : AsynchronousCollectionSettings {
        public readonly new static AsynchronousCollection2Settings Default = new AsynchronousCollection2Settings();
        public override RequestDataMode Mode { get { return RequestDataMode.OnDemand; } }
        public int BackgroundRequestDataRate { get; set; }
        public TimeSpan BackgroundRequestDataInterval { get; set; }

        public AsynchronousCollection2Settings()
            : base() {
            BackgroundRequestDataRate = 10;
            BackgroundRequestDataInterval = TimeSpan.FromSeconds(0.1);
        }
        public AsynchronousCollection2Settings(int requestDataRate)
            : base(requestDataRate) {
            BackgroundRequestDataRate = 10;
            BackgroundRequestDataInterval = TimeSpan.FromSeconds(0.1);
        }
        public AsynchronousCollection2Settings(int requestDataRate, int backgroundRequestDataRate, TimeSpan backgroundRequestDataInterval)
            : base(requestDataRate) {
            BackgroundRequestDataRate = backgroundRequestDataRate;
            BackgroundRequestDataInterval = backgroundRequestDataInterval;
        }
        protected internal override void Apply<T>(AsynchronousCollection<T> col) {
            base.Apply<T>(col);
            AsynchronousCollection2<T> col1 = (AsynchronousCollection2<T>)col;
            col1.BackgroundRequestDataRate = BackgroundRequestDataRate;
            col1.BackgroundRequestDataInterval = BackgroundRequestDataInterval;
        }
    }
}