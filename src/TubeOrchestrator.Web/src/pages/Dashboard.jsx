import React, { useState, useEffect } from 'react';
import ChannelCard from '../components/ChannelCard';
import JobMonitor from '../components/JobMonitor';
import { channelsApi } from '../services/api';

const Dashboard = () => {
  const [channels, setChannels] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const fetchChannels = async () => {
    try {
      const response = await channelsApi.getAll();
      setChannels(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to load channels');
      console.error('Error fetching channels:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchChannels();
  }, []);

  const handleJobTriggered = () => {
    // Increment to trigger JobMonitor refresh
    setRefreshTrigger((prev) => prev + 1);
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">
            TubeOrchestrator
          </h1>
          <p className="text-gray-600">
            Manage and automate your YouTube channel content
          </p>
        </div>

        {/* Channels Section */}
        <div className="mb-8">
          <h2 className="text-2xl font-bold text-gray-800 mb-4">Channels</h2>
          {isLoading ? (
            <div className="flex justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
          ) : error ? (
            <div className="bg-red-50 text-red-700 p-4 rounded-lg">{error}</div>
          ) : channels.length === 0 ? (
            <div className="bg-white rounded-lg shadow-md p-8 text-center text-gray-500">
              No channels found. Create one to get started!
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {channels.map((channel) => (
                <ChannelCard
                  key={channel.id}
                  channel={channel}
                  onJobTriggered={handleJobTriggered}
                />
              ))}
            </div>
          )}
        </div>

        {/* Jobs Monitor Section */}
        <JobMonitor refreshTrigger={refreshTrigger} />
      </div>
    </div>
  );
};

export default Dashboard;
