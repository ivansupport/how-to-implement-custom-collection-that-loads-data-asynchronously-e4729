using System.Collections;
using System.Diagnostics;
using System;

namespace AsyncDataLoading {
    public abstract class RequestArgs { }
    public abstract class RequestResult {
        public RequestArgs RequestArgs { get; private set; }

        public RequestResult(RequestArgs requestArgs) {
            if(requestArgs == null)
                throw new ArgumentNullException("requestArgs");
            RequestArgs = requestArgs;
        }
        public abstract void RaiseFeedback(object parameter);
    }

    public class RequestDataArgs : RequestArgs {
        public int SkipCount { get; private set; }
        public int TakeCount { get; private set; }
        public bool IsBackgroundRequest { get; private set; }

        public RequestDataArgs(int skipCount, int takeCount, bool isBackgroundRequest = false) {
            IsBackgroundRequest = isBackgroundRequest;
            SkipCount = skipCount;
            TakeCount = takeCount;
        }
        internal static readonly RequestDataArgs Empty = new RequestDataArgs(0, 0);
    }
    public class RequestDataResult : RequestResult {
        public IEnumerable Data { get; private set; }
        readonly RequestDataFeedback Feedback;

        internal RequestDataResult(RequestDataArgs requestArgs, RequestDataFeedback feedback)
            : base(requestArgs) {

            if(feedback == null)
                throw new ArgumentNullException("feedback");
            Feedback = feedback;
        }
        public override void RaiseFeedback(object data) {
            if(!(data is IEnumerable))
                throw new ArgumentException("The data parameter must be IEnumerable type");
            Data = (IEnumerable)data;
            Feedback(this);
        }
    }
    public delegate void RequestData(RequestDataArgs request, RequestDataResult feedback);
    delegate void RequestDataFeedback(RequestDataResult resultFeedback);

    public class RequestCountArgs : RequestArgs { }
    public class RequestCountResult : RequestResult {
        public int Count { get; private set; }
        readonly RequestCountFeedback Feedback;

        internal RequestCountResult(RequestCountArgs requestArgs, RequestCountFeedback feedback)
            : base(requestArgs) {
            if(feedback == null)
                throw new ArgumentNullException("feedback");
            Feedback = feedback;
        }
        public override void RaiseFeedback(object count) {
            if(!(count is int))
                throw new ArgumentException("The count parameter must be integer type");
            Count = (int)count;
            Feedback(this);
        }
    }
    public delegate void RequestCount(RequestCountArgs request, RequestCountResult feedback);
    delegate void RequestCountFeedback(RequestCountResult resultFeedback);

    public enum TypeSubmitChanges { Add, Remove, Clear, Replace, Update }
    public class SubmitChangesArgs : RequestArgs {
        public readonly TypeSubmitChanges Type;
        public object OldItem { get; private set; }
        public object NewItem { get; private set; }
        public int Index { get; private set; }

        public SubmitChangesArgs(TypeSubmitChanges Type) {
            OldItem = null;
            NewItem = null;
            Index = -1;
        }
        public SubmitChangesArgs(TypeSubmitChanges Type, object oldItem, object newItem, int index) {
            OldItem = oldItem;
            NewItem = newItem;
            Index = index;
        }
    }
    public class SubmitChangesResult : RequestResult {
        public bool Cancel { get; private set; }
        readonly SubmitChangesFeedback Feedback;
        internal SubmitChangesResult(SubmitChangesArgs requestArgs, SubmitChangesFeedback feedback)
            : base(requestArgs) {
                Feedback = feedback;
        }
        public override void RaiseFeedback(object cancel) {
            if(!(cancel is bool))
                throw new ArgumentException("The cancel parameter must be boolean type");
            Cancel = (bool)cancel;
            Feedback(this);
        }
    }
    public delegate void SubmitChanges(SubmitChangesArgs request, SubmitChangesResult feedback);
    delegate void SubmitChangesFeedback(SubmitChangesResult resultFeedback);
}
