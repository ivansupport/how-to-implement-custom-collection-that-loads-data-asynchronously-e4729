using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace AsyncDataLoading {
    public enum RequestDataMode { OnDemand, InBackgroundThread }
    public static class AsynchronousCollectionFactory {
        public static AsynchronousCollection<T> CreateCollection<T>(RequestCount requestCount, RequestData requestData, RequestDataMode requestDataMode) {
            if(requestDataMode == RequestDataMode.OnDemand) return new AsynchronousCollection<T>(requestCount, requestData);
            else return new AsynchronousCollection2<T>(requestCount, requestData);
        }
        public static AsynchronousCollection<T> CreateCollection<T>(RequestCount requestCount, RequestData requestData, AsynchronousCollectionSettings settings) {
            return CreateCollection<T>(requestCount, requestData, null, settings);
        }
        public static AsynchronousCollection<T> CreateCollection<T>(RequestCount requestCount, RequestData requestData, SubmitChanges submitChanges, AsynchronousCollectionSettings settings) {
            if(settings.GetType() == typeof(AsynchronousCollection2Settings))
                return new AsynchronousCollection2<T>(requestCount, requestData, submitChanges, (AsynchronousCollection2Settings)settings);
            if(settings.GetType() == typeof(AsynchronousCollectionSettings))
                return new AsynchronousCollection<T>(requestCount, requestData, submitChanges, settings);
            return null;
        }
    }
}
