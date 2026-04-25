export const StepperIndicator = ({ steps = [], currentStep = 0 }) => {
  return (
    <div className="flex flex-wrap items-center gap-2">
      {steps.map((step, index) => (
        <div key={step} className="flex items-center gap-2">
          <span className={`flex h-7 w-7 items-center justify-center rounded-full text-xs font-semibold ${index <= currentStep ? 'bg-primary text-on-primary' : 'bg-surface-dim text-on-background/70'}`}>
            {index + 1}
          </span>
          <span className={`text-sm ${index <= currentStep ? 'text-on-background font-medium' : 'text-on-background/60'}`}>{step}</span>
        </div>
      ))}
    </div>
  )
}

export default StepperIndicator
