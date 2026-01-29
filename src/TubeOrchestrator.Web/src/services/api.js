import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Channels API
export const channelsApi = {
  getAll: () => api.get('/channels'),
  getActive: () => api.get('/channels/active'),
  getById: (id) => api.get(`/channels/${id}`),
  create: (channel) => api.post('/channels', channel),
  update: (id, channel) => api.put(`/channels/${id}`, channel),
  delete: (id) => api.delete(`/channels/${id}`),
};

// Jobs API
export const jobsApi = {
  getAll: () => api.get('/jobs'),
  getRecent: (count = 10) => api.get(`/jobs/recent?count=${count}`),
  getById: (id) => api.get(`/jobs/${id}`),
  trigger: (channelId) => api.post(`/jobs/trigger/${channelId}`),
};

export default api;
