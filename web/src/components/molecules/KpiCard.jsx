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
  const isPositive = trendDirection === 'up'
  const trendColor = isPositive ? 'text-emerald-600 bg-emerald-50' : 'text-rose-500 bg-rose-50'
  const trendIcon  = isPositive ? 'trending_up' : 'trending_down'

  return (
    <div className="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-all duration-200 flex flex-col gap-4 group hover:-translate-y-0.5">
      <div className="flex justify-between items-start">
        {/* Icon container */}
        <div className={`w-11 h-11 ${iconBg} ${iconColor} rounded-xl flex items-center justify-center shadow-sm group-hover:scale-110 transition-transform`}>
          <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 1" }}>{icon}</span>
        </div>

        {/* Trend badge */}
        {trend !== null && (
          <span className={`flex items-center gap-0.5 text-xs font-bold ${trendColor} px-2.5 py-1 rounded-full`}>
            <span className="material-symbols-outlined text-[14px]">{trendIcon}</span>
            {trend}
          </span>
        )}
      </div>

      <div>
        <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide mb-1.5">{label}</p>
        <h2 className="text-2xl font-extrabold text-slate-900 leading-none">
          {prefix}{value}{suffix}
        </h2>
      </div>

      {/* Subtle bottom accent */}
      <div className="h-1 rounded-full bg-slate-100 overflow-hidden">
        <div className={`h-full w-3/4 rounded-full ${iconBg.replace('50', '200')} opacity-60`} />
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
