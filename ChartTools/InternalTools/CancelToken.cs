﻿namespace ChartTools
{
    internal class CancelToken
    {
        public bool CancelRquested { get; private set; } = false;
        public void Cancel() => CancelRquested = true;
    }
}