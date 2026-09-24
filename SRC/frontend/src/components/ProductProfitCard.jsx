import React, { useState } from 'react';
import axios from 'axios';
import { FiDollarSign, FiSearch, FiBarChart2, FiArrowUp } from 'react-icons/fi';
import StatCard from './StatCard';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ;

const ProductProfitCard = () => {
  const [productProfit, setProductProfit] = useState(null);
  const [productName, setProductName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleGetProductProfit = async () => {
    if (!productName.trim()) {
      setMessage('Please enter a product name.');
      return;
    }

    setIsLoading(true);
    setProductProfit(null);
    try {
      const response = await axios.get(`${API_BASE_URL}/GetProfit/${productName}`);
      setProductProfit(response.data);
      setMessage(response.data ? '' : 'No profit data available for this product.');
    } catch (error) {
      setProductProfit(null);
      setMessage(error.response?.data?.message || 'Error fetching product profit. Please try again.');
      console.error('Error fetching product profit:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const avgProfitPerSale = productProfit
    ? (productProfit.totalProfit / productProfit.numberOfSales).toFixed(2)
    : null;

  return (
    <StatCard
      title="Profit Calculator"
      icon={<FiDollarSign size={18} />}
      message={message}
      isLoading={isLoading}
    >
      <div style={{ marginTop: '0.25rem', display: 'flex', flexDirection: 'column', gap: '0.875rem' }}>
        {/* Search input */}
        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 600, color: '#64748b', marginBottom: '0.5rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            Product Name
          </label>
          <div style={{ position: 'relative' }}>
            <FiSearch size={14} style={{ position: 'absolute', left: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: '#475569', pointerEvents: 'none' }} />
            <input
              type="text"
              id="profitProductName"
              value={productName}
              onChange={(e) => setProductName(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleGetProductProfit()}
              placeholder="e.g. Product ABC"
              className="input-field"
              disabled={isLoading}
              style={{ paddingLeft: '2.25rem' }}
            />
          </div>
        </div>

        <button
          onClick={handleGetProductProfit}
          disabled={!productName.trim() || isLoading}
          className="btn-primary"
        >
          {isLoading ? (
            <>
              <div className="spinner" />
              Calculating...
            </>
          ) : (
            <>
              <FiBarChart2 size={15} />
              Calculate Profit
            </>
          )}
        </button>

        {/* Results */}
        {productProfit && (
          <div style={{
            background: 'rgba(255,255,255,0.02)',
            border: '1px solid #1e2d45',
            borderRadius: '12px',
            overflow: 'hidden',
          }}>
            <div style={{
              padding: '0.75rem 1rem',
              borderBottom: '1px solid #1e2d45',
              background: 'rgba(99,102,241,0.05)',
            }}>
              <p style={{ margin: 0, fontSize: '0.75rem', fontWeight: 700, color: '#6366f1', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
                Profit Analysis
              </p>
            </div>

            <div style={{ padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.82rem', color: '#64748b' }}>Total Profit</span>
                <span style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f1f5f9' }}>
                  EGP {parseFloat(productProfit.totalProfit).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.82rem', color: '#64748b' }}>Number of Sales</span>
                <span style={{ fontSize: '0.95rem', fontWeight: 700, color: '#f1f5f9' }}>
                  {productProfit.numberOfSales.toLocaleString()}
                </span>
              </div>

              <div style={{ height: '1px', background: '#1e2d45' }} />

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.82rem', color: '#94a3b8', fontWeight: 600 }}>Avg. Per Sale</span>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                  <FiArrowUp size={12} style={{ color: '#10b981' }} />
                  <span style={{ fontSize: '1rem', fontWeight: 800, color: '#34d399' }}>
                    EGP {avgProfitPerSale}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </StatCard>
  );
};

export default ProductProfitCard;
