
using Acacia.Features.DebugSupport;
/// Copyright 2016 Kopano b.v.
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU Affero General Public License for more details.
/// 
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.
/// 
/// Consult LICENSE file for details
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public class TasksBackground : TaskExecutor
    {
        private readonly BlockingCollection<AcaciaTask> _tasks = new BlockingCollection<AcaciaTask>();

        public TasksBackground()
        {
            Thread t = new Thread(Worker);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void Worker()
        {
            while (!_tasks.IsCompleted)
            {
                AcaciaTask task = _tasks.Take();
                PerformTask(task);
            }
        }

        protected override void EnqueueTask(AcaciaTask task)
        {
            _tasks.Add(task);
        }

        override public string Name { get { return "Background"; } }
    }
}
