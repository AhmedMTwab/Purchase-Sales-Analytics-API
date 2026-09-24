import { Link, useLocation } from 'react-router-dom';
import { FiBarChart2, FiPackage, FiUploadCloud, FiActivity, FiZap } from 'react-icons/fi';

const navLinks = [
  { to: '/', label: 'Dashboard', icon: FiActivity },
  { to: '/analysis', label: 'Analysis', icon: FiBarChart2 },
  { to: '/products', label: 'Products', icon: FiPackage },
  { to: '/upload', label: 'Upload Data', icon: FiUploadCloud },
];

const Navbar = () => {
  const location = useLocation();

  const isActive = (path) => {
    if (path === '/' && location.pathname !== '/') return false;
    return location.pathname === path || (path !== '/' && location.pathname.startsWith(path));
  };

  return (
    <nav style={{
      background: 'rgba(17, 24, 39, 0.85)',
      backdropFilter: 'blur(20px)',
      borderBottom: '1px solid #1e2d45',
      position: 'sticky',
      top: 0,
      zIndex: 100,
    }}>
      <div style={{ maxWidth: '1280px', margin: '0 auto', padding: '0 1.5rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', height: '64px' }}>
          
          {/* Logo */}
          <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: '0.625rem', textDecoration: 'none' }}>
            <div style={{
              width: '34px', height: '34px',
              background: 'linear-gradient(135deg, #6366f1, #4f46e5)',
              borderRadius: '9px',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              boxShadow: '0 4px 12px rgba(99,102,241,0.4)',
            }}>
              <FiZap size={16} color="white" />
            </div>
            <span style={{
              fontWeight: 800,
              fontSize: '1.05rem',
              letterSpacing: '-0.02em',
              background: 'linear-gradient(135deg, #f1f5f9, #818cf8)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}>
              P&S
            </span>
          </Link>

          {/* Nav Links */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
            {navLinks.map(({ to, label, icon: Icon }) => (
              <Link
                key={to}
                to={to}
                className={`nav-link ${isActive(to) ? 'active' : ''}`}
              >
                <Icon size={15} />
                {label}
              </Link>
            ))}
          </div>

          {/* Status indicator */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div className="glow-dot" />
            <span style={{ fontSize: '0.75rem', color: '#64748b', fontWeight: 500 }}>Live</span>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;