import { Button } from '@/components/ui/button'

export function HomePage() {
  return (
    <section className="mx-auto flex max-w-3xl flex-col gap-6 px-6 py-16 text-center">
      <h1 className="text-4xl font-semibold tracking-tight md:text-5xl">Coursely</h1>
      <p className="text-muted-foreground text-lg">
        Frontend foundation — roadmap Phase 0.1 (React, Vite, TanStack Query,
        Zustand).
      </p>
      <div className="flex flex-wrap justify-center gap-3">
        <Button type="button">Get started</Button>
        <Button type="button" variant="outline">
          Explore
        </Button>
      </div>
    </section>
  )
}
