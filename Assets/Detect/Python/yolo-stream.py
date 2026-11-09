import cv2
from ultralytics import YOLO
import pyvirtualcam

# 1. Load specialized face detection model (yolov8n-face is fast and lightweight)
# model = YOLO('yolov8n-face.pt') 
model = YOLO('yolov8n.pt') 

# 2. Open your video source (change to 0 if reading from a physical webcam instead)
video_path = "people-street.mp4" 
cap = cv2.VideoCapture(video_path)

width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
fps = int(cap.get(cv2.CAP_PROP_FPS)) or 30

# 3. Initialize Virtual Camera for OBS to ingest
with pyvirtualcam.Camera(
    width=width, 
    height=height, 
    fps=fps, 
    backend="obs",
    device="OBS Virtual Camera",
    # delay_dict=None
    ) as cam:
    print(f"Virtual Camera streaming tracking data: {cam.device}")

    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
            continue
            # break
            
        # Run face inference
        results = model(frame, conf=0.4, verbose=False)[0]
        
        # Plot default bounding boxes and keypoints onto the frame
        annotated_frame = results.plot()
        
        # PyVirtualCam requires RGB, OpenCV operates in BGR
        send_frame = cv2.cvtColor(annotated_frame, cv2.COLOR_BGR2RGB)
        
        # Send to Virtual Camera
        cam.send(send_frame)
        cam.sleep_until_next_frame()

cap.release()
