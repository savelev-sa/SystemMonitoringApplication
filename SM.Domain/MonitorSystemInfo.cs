﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SM.Model;

namespace SM.Domain
{
    /// <summary>
    /// Модель содержит информацию о состоянии системы
    /// </summary>
    public class MonitorSystemInfo
    {
        public MonitorSystemInfo()
        {
            SystemInfo = new SystemInfo();
            _cpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramPerformanceCounter = new PerformanceCounter("Memory", "Available MBytes");

            Refresh();

        }

        /// <summary>
        /// Оповещает о высокой нагрузке
        /// </summary>
        public event EventHandler OnHighLoaded;

        /// <summary>
        /// Обновить состояние
        /// </summary>
        public void Refresh()
        {
            SystemInfo.TotalCpuUsagePercent = Math.Round(_cpuPerformanceCounter.NextValue(), 2);

            //Получаем количество установленной памяти в kb
            GetPhysicallyInstalledSystemMemory(out var totalMemoryKb);
            // приводим к Mb
            var totalMemoryMb = totalMemoryKb / 1024;

            //получаем количество доступной памяти в Mb
            var systemAvailableMemoryValue = _ramPerformanceCounter.NextValue();

            SystemInfo.TotalMemoryUsageMb = totalMemoryMb - systemAvailableMemoryValue;

            SystemInfo.TotalMemoryUsagePercent = Math.Round(SystemInfo.TotalMemoryUsageMb * 100 / totalMemoryMb, 2);

            //проверяем состояние системы
            CheckState();
        }

        private void CheckState()
        {
            //если процент нагрузки выше порогового значения
            if (SystemInfo.TotalCpuUsagePercent > HighLoadPercentage
            && SystemInfo.TotalMemoryUsagePercent > HighLoadPercentage)
            {// оповещаем о высокой нагрузке
                OnHighLoaded?.Invoke(sender: this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Пороговое значение высокой нагрузки
        /// </summary>
        private int HighLoadPercentage = 85;

        private readonly PerformanceCounter _cpuPerformanceCounter;
        private readonly PerformanceCounter _ramPerformanceCounter;
        private readonly SystemInfo SystemInfo;


        /// <summary>
        /// Получить количество установленной оперативной памяти
        /// </summary>
        /// <param name="totalMemoryInKilobytes">Количество установленной памяти</param>
        /// <returns>Возвращаемое значение в Kb</returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
    }
}