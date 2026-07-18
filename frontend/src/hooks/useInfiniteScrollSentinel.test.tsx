import { render } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useInfiniteScrollSentinel } from './useInfiniteScrollSentinel';

describe('useInfiniteScrollSentinel', () => {
  let observeMock: ReturnType<typeof vi.fn>;
  let disconnectMock: ReturnType<typeof vi.fn>;
  let capturedCallback: IntersectionObserverCallback | undefined;
  let originalIntersectionObserver: typeof IntersectionObserver;

  beforeEach(() => {
    originalIntersectionObserver = globalThis.IntersectionObserver;
    observeMock = vi.fn();
    disconnectMock = vi.fn();

    class MockIntersectionObserver {
      constructor(callback: IntersectionObserverCallback) {
        capturedCallback = callback;
      }
      observe = observeMock;
      disconnect = disconnectMock;
      unobserve = vi.fn();
      takeRecords = vi.fn(() => []);
      root = null;
      rootMargin = '';
      thresholds = [];
    }

    globalThis.IntersectionObserver = MockIntersectionObserver as unknown as typeof IntersectionObserver;
  });

  afterEach(() => {
    globalThis.IntersectionObserver = originalIntersectionObserver;
    capturedCallback = undefined;
  });

  function TestComponent({ onIntersect, enabled }: { onIntersect: () => void; enabled: boolean }) {
    const ref = useInfiniteScrollSentinel(onIntersect, enabled);
    return <div ref={ref} data-testid="sentinel" />;
  }

  it('observes the sentinel element when enabled', () => {
    render(<TestComponent onIntersect={() => {}} enabled={true} />);

    expect(observeMock).toHaveBeenCalledTimes(1);
  });

  it('does not observe when disabled', () => {
    render(<TestComponent onIntersect={() => {}} enabled={false} />);

    expect(observeMock).not.toHaveBeenCalled();
  });

  it('calls onIntersect when the sentinel intersects', () => {
    const onIntersect = vi.fn();
    render(<TestComponent onIntersect={onIntersect} enabled={true} />);

    capturedCallback?.(
      [{ isIntersecting: true } as IntersectionObserverEntry],
      {} as IntersectionObserver,
    );

    expect(onIntersect).toHaveBeenCalledTimes(1);
  });

  it('does not call onIntersect when the sentinel is not intersecting', () => {
    const onIntersect = vi.fn();
    render(<TestComponent onIntersect={onIntersect} enabled={true} />);

    capturedCallback?.(
      [{ isIntersecting: false } as IntersectionObserverEntry],
      {} as IntersectionObserver,
    );

    expect(onIntersect).not.toHaveBeenCalled();
  });

  it('disconnects the observer on unmount', () => {
    const { unmount } = render(<TestComponent onIntersect={() => {}} enabled={true} />);

    unmount();

    expect(disconnectMock).toHaveBeenCalledTimes(1);
  });
});
