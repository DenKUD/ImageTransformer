using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kontur.ImageTransformer
{
    public class PerformanceCounter
    {
        private Queue<long> _execTime;
        private long _averageTime;
        private int _countQuantity;
        private int _frequencyOfAvgCalculation;
        private int _countsSinceLastAvgCalculation;
        
        public PerformanceCounter(int countQuantity, int frequncy)
        {
            _countQuantity = countQuantity;
            _countsSinceLastAvgCalculation = 0;
            _averageTime = 0;
            _frequencyOfAvgCalculation = frequncy;
            _execTime = new Queue<long>(_countQuantity);
            for (int i = 0; i < _countQuantity; i++)
                _execTime.Enqueue(0);
        }

        public async Task AddTime(long t)
        {
            lock(_execTime)
            {
                _execTime.Dequeue();
                _execTime.Enqueue(t);
                _countsSinceLastAvgCalculation++;
            };
            if (_countsSinceLastAvgCalculation >= _frequencyOfAvgCalculation)
                 CalculateAvgTime();

        }
        private async Task CalculateAvgTime()
        {
            lock(_execTime)
            {
                _averageTime=_execTime.Sum()/_countQuantity;
                _countsSinceLastAvgCalculation = 0;
            }
        }
        public long GetAvgTime()
        {
            return _averageTime;
        }
        
    }
}
