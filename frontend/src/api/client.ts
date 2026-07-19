import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8080',
  // Axios defaults to no timeout: a hung request would never reject, so mutation
  // error paths would never run and the UI (e.g. text-widget save mode, which
  // disables Save/Cancel while saving) would stay locked forever. 10s bounds that.
  timeout: 10_000,
});
