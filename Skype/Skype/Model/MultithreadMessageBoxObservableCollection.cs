using Skype.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Skype.Resources.Model
{
    public class MultithreadMessageBoxObservableCollection : ObservableCollection<MessageSection>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Use BlockReentrancy
            using (BlockReentrancy())
            {
                var eventHandler = CollectionChanged;

                // Only proceed if handler exists
                if (eventHandler != null)
                {
                    Delegate[] delegates = eventHandler.GetInvocationList();

                    // Walk thru invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        var currentDispatcher = handler.Target as DispatcherObject;

                        // If the subscriber is a DispatcherObject and different thread
                        if ((currentDispatcher != null) &&
                            (currentDispatcher.CheckAccess() == false))
                        {
                            // Invoke handler in the target dispatcher's thread
                            currentDispatcher.Dispatcher.Invoke(
                                DispatcherPriority.DataBind, handler, this, e);
                        }
                        else
                        {
                            handler(this, e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Overridden NotifyCollectionChangedEventHandler event
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
