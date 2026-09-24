import React, { useState } from 'react';
import axios from 'axios';
import { FiTrendingUp } from 'react-icons/fi';
import StatCard from './StatCard';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

const TopSalesCard = () => {
  const [topSales, setTopSales] = useState([]);
  const [numberOfProducts, setNumberOfProducts] = useState(5);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleGetTopSales = async () => {
    if (numberOfProducts < 1) {
      setMessage('Please enter a number greater than 0.');
      return;
    }

    setIsLoading(true);
    try {
      const response = await axios.get(`${API_BASE_URL}/GetTopSales/${numberOfProducts}`);
      setTopSales(response.data);
      setMessage(response.data.length ? '' : 'No top sales data available.');
    } catch (error) {
      setTopSales([]);
      setMessage(error.response?.data?.message || 'Error fetching top sales. Please try again.');
      console.error('Error fetching top sales:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getRankClass = (index) => {
    if (index === 0) return 'rank-1';
    if (index === 1) return 'rank-2';
    if (index === 2) return 'rank-3';
    return 'rank-other';
  };

  return (
    <StatCard
      title="Top Performing Products"
      icon={<FiTrendingUp size={18} />}
      message={message}
      isLoading={isLoading}
      customContent={
        <>
          {/* Controls */}
          <div style={{ marginTop: '0.25rem' }}>
            <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 600, color: '#64748b', marginBottom: '0.5rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              Show Top N Products
            </label>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <input
                type="number"
                id="productCount"
                min="1"
                max="50"
                value={numberOfProducts}
                onChange={(e) => setNumberOfProducts(Math.max(1, Math.min(50, parseInt(e.target.value) || 1)))}
                className="input-field"
                disabled={isLoading}
                style={{ maxWidth: '90px', textAlign: 'center', fontWeight: 700 }}
              />
              <button
                onClick={handleGetTopSales}
                disabled={isLoading}
                className="btn-primary"
                style={{ flex: 1 }}
              >
                {isLoading ? (
                  <>
                    <div className="spinner" />
                    Loading...
                  </>
                ) : 'Fetch Rankings'}
              </button>
            </div>
          </div>

          {/* Results */}
          {topSales.length > 0 && (
            <div style={{ marginTop: '1.25rem' }}>
              <div className="divider" style={{ margin: '0 0 1rem 0' }} />
              <p className="section-label">Top {topSales.length} Results</p>
              <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: '0.5rem', maxHeight: '260px', overflowY: 'auto', paddingRight: '4px' }}>
                {topSales.map((product, index) => (
                  <li key={index} style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.75rem',
                    padding: '0.625rem 0.875rem',
                    background: 'rgba(255,255,255,0.03)',
                    borderRadius: '10px',
                    border: '1px solid rgba(30,45,69,0.6)',
                    transition: 'all 0.2s ease',
                  }}>
                    <span className={`rank-badge ${getRankClass(index)}`}>{index + 1}</span>
                    <span style={{ fontSize: '0.85rem', color: '#cbd5e1', fontWeight: 500, flex: 1, minWidth: 0, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {product}
                    </span>
                    {index < 3 && (
                      <FiTrendingUp size={13} style={{ color: index === 0 ? '#fbbf24' : '#64748b', flexShrink: 0 }} />
                    )}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </>
      }
    />
  );
};

export default TopSalesCard;
