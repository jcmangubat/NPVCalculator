import React from "react";

function ResultsTable({ data }) {
  return (
    <div>
      <h2>NPV Results Table</h2>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            <th style={{ borderBottom: "1px solid #ccc", textAlign: "left" }}>
              Discount Rate (%)
            </th>
            <th style={{ borderBottom: "1px solid #ccc", textAlign: "left" }}>
              NPV
            </th>
          </tr>
        </thead>
        <tbody>
          {data.map((entry, index) => (
            <tr key={index}>
              <td data-label="Discount Rate (%)">
                {entry.discountRate.toFixed(2)}
              </td>
              <td data-label="NPV">{entry.npv.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default ResultsTable;
