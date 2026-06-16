import { useState } from 'react';

export function useAsync() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [result, setResult] = useState(null);

  const run = async (fn) => {
    setLoading(true);
    setError('');
    try {
      const data = await fn();
      setResult(data);
      return data;
    } catch (e) {
      setError(e.message ?? String(e));
      setResult(null);
      throw e;
    } finally {
      setLoading(false);
    }
  };

  return { loading, error, result, run, setResult, setError };
}

export function ResultBox({ error, result }) {
  if (error) return <div className="error">{error}</div>;
  if (result != null) return <pre className="result">{JSON.stringify(result, null, 2)}</pre>;
  return null;
}

export function DataTable({ rows, columns, onRowClick }) {
  if (!rows?.length) return <p className="hint">No data.</p>;
  return (
    <table>
      <thead>
        <tr>
          {columns.map((c) => (
            <th key={c.key}>{c.label}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((row, i) => (
          <tr
            key={row.id ?? i}
            className={onRowClick ? 'clickable' : ''}
            onClick={() => onRowClick?.(row)}
          >
            {columns.map((c) => (
              <td key={c.key}>{c.render ? c.render(row) : row[c.key]}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
