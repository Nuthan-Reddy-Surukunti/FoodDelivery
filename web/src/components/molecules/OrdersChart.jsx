import PropTypes from 'prop-types'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

export const OrdersChart = ({ data = [], title = 'Orders Over Time', height = 300 }) => {
  // Default data if none provided
  const chartData = data.length > 0 ? data : [
    { day: 'Mon', orders: 20 },
    { day: 'Tue', orders: 35 },
    { day: 'Wed', orders: 50 },
    { day: 'Thu', orders: 45 },
    { day: 'Fri', orders: 60 },
    { day: 'Sat', orders: 80 },
    { day: 'Sun', orders: 85 },
  ]

  return (
    <div className="bg-surface-container-lowest p-6 rounded-xl border border-surface-variant shadow-sm flex flex-col">
      <div className="flex justify-between items-center mb-6">
        <h3 className="font-headline-md text-headline-md text-on-surface text-[20px] leading-tight">{title}</h3>
        <button className="text-primary font-label-md text-label-md flex items-center hover:bg-surface-container px-3 py-1.5 rounded-lg transition-colors">
          Last 7 Days <span className="material-symbols-outlined text-[20px] ml-1">expand_more</span>
        </button>
      </div>

      <div style={{ height: `${height}px` }} className="w-full">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
            <defs>
              <linearGradient id="colorOrders" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#0a73e0" stopOpacity={0.4} />
                <stop offset="95%" stopColor="#0a73e0" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="#e0e2eb" vertical={false} />
            <XAxis 
              dataKey="day" 
              stroke="#717785" 
              style={{ fontSize: '12px', fontWeight: '500' }}
            />
            <YAxis 
              stroke="#717785"
              style={{ fontSize: '12px', fontWeight: '500' }}
            />
            <Tooltip 
              contentStyle={{
                backgroundColor: '#ffffff',
                border: '1px solid #e0e2eb',
                borderRadius: '8px',
                boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
              }}
              labelStyle={{ color: '#181c22' }}
              formatter={(value) => [`${value} orders`, 'Orders']}
            />
            <Line
              type="monotone"
              dataKey="orders"
              stroke="#005ab4"
              strokeWidth={2}
              dot={{ fill: '#ffffff', stroke: '#005ab4', r: 4, strokeWidth: 1 }}
              activeDot={{ r: 6 }}
              fillOpacity={1}
              fill="url(#colorOrders)"
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}

OrdersChart.propTypes = {
  data: PropTypes.arrayOf(PropTypes.shape({
    day: PropTypes.string,
    orders: PropTypes.number,
  })),
  title: PropTypes.string,
  height: PropTypes.number,
}
