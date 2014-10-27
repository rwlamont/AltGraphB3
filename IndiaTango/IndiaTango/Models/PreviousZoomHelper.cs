using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using IndiaTango.ViewModels;
using Visiblox.Charts;
using Visiblox.Charts.Primitives;


namespace IndiaTango.Models
{
    public class ZoomState
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private DoubleRange _range;

        public ZoomState(DateTime StartTime_2, DateTime EndTime_2, DoubleRange Range_2)
        {
            this.StartTime = StartTime_2;
            this.EndTime = EndTime_2;
            this.Range = Range_2;
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
            }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
            }
        }

        public DoubleRange Range
        {
            get { return _range; }
            set
            {
                _range = value;
            }
        }
    }

    public class PreviousZoomHelper
    {
         private LinkedList<ZoomState> _previousZooms = new LinkedList<ZoomState>();
         private int _count = 0;
        /// <summary>
        /// Adds a new zoom to the list, will remove oldest if there are already 3
        /// </summary>
        /// <param name="nuZoom">zoom to be added</param>
        public void Add(ZoomState nuZoom)
         {
             if (_previousZooms.Count >= 4)
             {
                 _previousZooms.RemoveLast();
                 _previousZooms.AddFirst(nuZoom);
             }
             else
             {
                 _previousZooms.AddFirst(nuZoom);
             }
             this._count = _previousZooms.Count;
         }
        /// <summary>
        /// gets the last zooom and deletes it
        /// </summary>
        /// <returns>the previous zoom</returns>
        public ZoomState GetLast()
        {

            ZoomState toReturn = _previousZooms.First();
            _previousZooms.RemoveFirst();
            this._count = _previousZooms.Count;
            return toReturn;

        }

        /// <summary>
        /// Clears all previous zoom states
        /// </summary>
        public void Clear()
        {
            _previousZooms.Clear();
            this._count = _previousZooms.Count;
        }

        /// <summary>
        /// The number of currently queued zoom states
        /// </summary>
        public int Count { get { return _count; }
            set
            {
                _count = value;
            }
            
            }
    }
}
