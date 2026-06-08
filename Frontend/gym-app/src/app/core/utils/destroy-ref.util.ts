import { DestroyRef, inject } from '@angular/core';
import { Observable, MonoTypeOperatorFunction } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export function injectDestroyRef(): DestroyRef {
  return inject(DestroyRef);
}

export function untilDestroyed<T>(): MonoTypeOperatorFunction<T> {
  return takeUntilDestroyed<T>(injectDestroyRef());
}

export function takeUntilDestroyedFrom<T>(destroyRef?: DestroyRef): MonoTypeOperatorFunction<T> {
  return destroyRef ? takeUntilDestroyed<T>(destroyRef) : untilDestroyed<T>();
}
