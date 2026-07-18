import { render, screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from './test/msw/server';
import App from './App';

const API_URL = 'http://localhost:8080';

describe('App', () => {
  it('renders the dashboard', async () => {
    server.use(http.get(`${API_URL}/api/widgets`, () => HttpResponse.json([])));

    render(<App />);

    expect(await screen.findByText('Dashboard')).toBeInTheDocument();
  });
});
