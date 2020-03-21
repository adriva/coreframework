using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;

namespace Adriva.Analytics.Abstractions
{
    internal class TelemetryBuffer
    {
        public Action OnFull;
        private const int DefaultCapacity = 500;
        private const int DefaultBacklogSize = 1000000;
        private readonly object LockObject = new object();
        private int CapacityField = DefaultCapacity;
        private int BacklogSizeField = DefaultBacklogSize;
        private int MinimumBacklogSize = 1001;
        private List<ITelemetry> Items;
        private bool IsItemDroppedMessageLogged = false;

        internal TelemetryBuffer()
        {
            this.Items = new List<ITelemetry>(this.Capacity);
        }

        /// <summary>
        /// Gets or sets the maximum number of telemetry items that can be buffered before transmission.
        /// </summary>        
        public int Capacity
        {
            get
            {
                return this.CapacityField;
            }

            set
            {
                if (value < 1)
                {
                    this.CapacityField = DefaultCapacity;
                    return;
                }

                if (value > this.BacklogSizeField)
                {
                    this.CapacityField = this.BacklogSizeField;
                    return;
                }

                this.CapacityField = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of telemetry items that can be in the backlog to send. Items will be dropped
        /// once this limit is hit.
        /// </summary>        
        public int BacklogSize
        {
            get
            {
                return this.BacklogSizeField;
            }

            set
            {
                if (value < this.MinimumBacklogSize)
                {
                    this.BacklogSizeField = this.MinimumBacklogSize;
                    return;
                }

                if (value < this.CapacityField)
                {
                    this.BacklogSizeField = this.CapacityField;
                    return;
                }

                this.BacklogSizeField = value;
            }
        }

        public void Enqueue(ITelemetry item)
        {
            if (item == null)
            {
                //CoreEventSource.Log.LogVerbose("item is null in TelemetryBuffer.Enqueue");
                return;
            }

            lock (this.LockObject)
            {
                if (this.Items.Count >= this.BacklogSize)
                {
                    if (!this.IsItemDroppedMessageLogged)
                    {
                        this.IsItemDroppedMessageLogged = true;
                    }

                    return;
                }

                this.Items.Add(item);
                if (this.Items.Count >= this.Capacity)
                {
                    var onFull = this.OnFull;
                    if (onFull != null)
                    {
                        onFull();
                    }
                }
            }
        }

        public virtual IEnumerable<ITelemetry> Dequeue()
        {
            List<ITelemetry> telemetryToFlush = null;

            if (this.Items.Count > 0)
            {
                lock (this.LockObject)
                {
                    if (this.Items.Count > 0)
                    {
                        telemetryToFlush = this.Items;
                        this.Items = new List<ITelemetry>(this.Capacity);
                        this.IsItemDroppedMessageLogged = false;
                    }
                }
            }

            return telemetryToFlush;
        }
    }
}