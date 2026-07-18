import { useEffect, useRef } from 'react';

/** Returns a ref to attach to a sentinel element; calls onIntersect when it scrolls into view. */
export function useInfiniteScrollSentinel(onIntersect: () => void, enabled: boolean) {
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const node = sentinelRef.current;
    if (!node) {
      return;
    }

    const observer = new IntersectionObserver((entries) => {
      if (entries[0]?.isIntersecting) {
        onIntersect();
      }
    });
    observer.observe(node);

    return () => observer.disconnect();
  }, [onIntersect, enabled]);

  return sentinelRef;
}
