from ultralytics import YOLO

# Load the PyTorch model
model = YOLO('yolov8n.pt')

# Export to ONNX with explicit dimensions 
# YOLOv8 default shape is [1, 84, 8400] (for 80 COCO classes + 4 box coords)
model.export(format='onnx', imgsz=640, opset=12)
