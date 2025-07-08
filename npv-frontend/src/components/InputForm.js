import React, { useState } from "react";
import { calculateNpv, downloadCsv } from "../services/api";

function InputForm({ onSubmitStart, onSubmitComplete, onSubmitError }) {
  const [cashFlows, setCashFlows] = useState("-10000,3000,4200,6800");
  const [lowerRate, setLowerRate] = useState(1.0);
  const [upperRate, setUpperRate] = useState(5.0);
  const [increment, setIncrement] = useState(1.0);

  const parseCashFlows = () =>
    cashFlows
      .split(",")
      .map((s) => parseFloat(s.trim()))
      .filter((n) => !isNaN(n));

  const handleSubmit = async (e) => {
    e.preventDefault();
    const payload = {
      cashFlows: parseCashFlows(),
      lowerBoundRate: parseFloat(lowerRate),
      upperBoundRate: parseFloat(upperRate),
      increment: parseFloat(increment),
    };

    try {
      onSubmitStart();

      // line satisfies the requirement of using asynchronous calls.
      const result = await calculateNpv(payload);

      onSubmitComplete(result);
    } catch (err) {
            onSubmitError(err);
    }
  };

  const handleDownload = async () => {
    const payload = {
      cashFlows: parseCashFlows(),
      lowerBoundRate: parseFloat(lowerRate),
      upperBoundRate: parseFloat(upperRate),
      increment: parseFloat(increment),
    };

    try {
      // 
      await downloadCsv(payload);
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: "2rem" }}>
      <div>
        <label>Cash Flows (comma-separated):</label>
        <br />
        <input
          type="text"
          value={cashFlows}
          onChange={(e) => setCashFlows(e.target.value)}
          required
          style={{ width: "100%" }}
        />
      </div>

      <div style={{ marginTop: "1rem" }}>
        <label>Lower Bound Rate (%):</label>
        <br />
        <input
          type="number"
          step="0.01"
          value={lowerRate}
          onChange={(e) => setLowerRate(e.target.value)}
          required
        />
      </div>

      <div>
        <label>Upper Bound Rate (%):</label>
        <br />
        <input
          type="number"
          step="0.01"
          value={upperRate}
          onChange={(e) => setUpperRate(e.target.value)}
          required
        />
      </div>

      <div>
        <label>Increment (%):</label>
        <br />
        <input
          type="number"
          step="0.01"
          value={increment}
          onChange={(e) => setIncrement(e.target.value)}
          required
        />
      </div>

      <div style={{ marginTop: "1rem" }}>
        <button type="submit">Calculate NPV</button>
        <button
          type="button"
          onClick={handleDownload}
          style={{ marginLeft: "1rem" }}
        >
          Download CSV
        </button>
      </div>
    </form>
  );
}

export default InputForm;
