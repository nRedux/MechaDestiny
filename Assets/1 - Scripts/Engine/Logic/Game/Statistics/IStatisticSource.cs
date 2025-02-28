using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatisticSource
{
    Statistic AddStatistic( StatisticType statistic, int value );
    Statistic GetStatistic( StatisticType statistic, bool createIfMissing = false );
    bool HasStatistic( StatisticType statistic );
}
