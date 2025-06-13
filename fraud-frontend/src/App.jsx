import { useEffect, useState } from 'react';
export default function App() {
  const [alerts, setAlerts] = useState([]);
  useEffect(() => {
    fetch('/api/alerts')
      .then(r => r.json())
      .then(setAlerts);
  }, []);

  return (
    <div className="container">
      <h1>Fraud Alerts (last 50)</h1>
      <table>
        <thead><tr><th>ID</th><th>Amount</th><th>Timestamp</th></tr></thead>
        <tbody>
          {alerts.map(a => (
            <tr key={a.id}>
              <td>{a.transactionId}</td>
              <td>{a.amount.toLocaleString()}</td>
              <td>{new Date(a.timestamp).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}