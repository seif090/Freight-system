from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
from datetime import datetime, timedelta
from math import hypot

app = FastAPI(title="Freight ML Streaming Service")

class TrackPoint(BaseModel):
    Timestamp: datetime
    Latitude: float
    Longitude: float
    Status: str

class StreamRequest(BaseModel):
    ShipmentId: int
    TrackPoints: List[TrackPoint]

class AnomalyRequest(BaseModel):
    id: int
    trackingNumber: str
    status: str
    etd: Optional[datetime]
    eta: Optional[datetime]
    currentLatitude: Optional[float]
    currentLongitude: Optional[float]

class EtaRequest(BaseModel):
    id: int
    trackingNumber: str
    status: str
    etd: Optional[datetime]
    eta: Optional[datetime]
    routeSegments: Optional[List[dict]]

@app.post("/api/ml/stream")
async def stream_route_data(request: StreamRequest):
    # Here, you can produce to Kafka or insert into streaming data lake.
    # This is a placeholder that accepts telemetry and returns success.
    return {"success": True, "ingested": len(request.TrackPoints)}

@app.post("/api/ml/anomaly")
async def detect_anomaly(request: AnomalyRequest):
    if request.eta is None or request.etd is None:
        raise HTTPException(status_code=400, detail="ETD and ETA required")

    diff = (datetime.utcnow() - request.eta).total_seconds() / 60
    is_anomaly = diff > 30
    return {
        "ShipmentId": request.id,
        "IsAnomaly": is_anomaly,
        "DelayMinutes": max(0, diff),
        "Forecast": "delay" if is_anomaly else "on-time"
    }

@app.post("/api/ml/eta")
async def predict_eta(request: EtaRequest):
    now = datetime.utcnow()
    predicted_eta = (request.eta or now) + timedelta(minutes=15)
    return {
        "ShipmentId": request.id,
        "PredictedETA": predicted_eta,
        "Confidence": 0.83,
        "PredictedDelayMinutes": 15
    }
