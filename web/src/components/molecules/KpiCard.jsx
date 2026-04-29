import PropTypes from 'prop-types'

export const KpiCard = ({ 
  icon = 'bar_chart', 
  label = 'Metric', 
  value = '0', 
  trend = null,
  trendDirection = 'up',
  iconBg = 'bg-blue-50',
  iconColor = 'text-blue-600',
  prefix = '',
  suffix = '',
}) => {
  const isPositiveTrend = trendDirection === 'up'
  const trendColor = isPositiveTrend ? 'text-emerald-600 bg-emerald-50' : 'text-slate-500 bg-slate-100'
  const trendIcon = isPositiveTrend ? 'trending_up' : 'horizontal_rule'

  return (
    <div className="bg-surface-container-lowest p-6 rounded-xl border border-surface-variant shadow-sm flex flex-col gap-4 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start">
        <div className={`p-2 ${iconBg} ${iconColor} rounded-lg`}>
          <span className="material-symbols-outlined">{icon}</span>
        </div>
        {trend !== null && (
          <span className={`flex items-center text-sm font-medium ${trendColor} px-2 py-1 rounded-full`}>
            <span className="material-symbols-outlined text-[16px] mr-1">{trendIcon}</span>
            {trend}
          </span>
        )}
      </div>
      <div>
        <p className="font-label-md text-label-md text-on-surface-variant mb-1">{label}</p>
        <h2 className="font-headline-md text-headline-md text-on-surface">
          {prefix}{value}{suffix}
        </h2>
      </div>
    </div>
  )
}

KpiCard.propTypes = {
  icon: PropTypes.string,
  label: PropTypes.string,
  value: PropTypes.string,
  trend: PropTypes.string,
  trendDirection: PropTypes.oneOf(['up', 'down']),
  iconBg: PropTypes.string,
  iconColor: PropTypes.string,
  prefix: PropTypes.string,
  suffix: PropTypes.string,
}
