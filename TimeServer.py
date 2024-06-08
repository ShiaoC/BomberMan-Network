import socket
import threading
import time
from datetime import datetime

# 设置服务器地址和端口
server_address = ('localhost', 9999)

# 创建UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(server_address)

clients = set()
stop_event = threading.Event()

def handle_client():
    while not stop_event.is_set():
        current_time = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        for client in clients:
            try:
                sock.sendto(current_time.encode(), client)
                print(f"Sent: {current_time} to {client}")
            except OSError as e:
                print(f"Error sending data to {client}: {e}")
        time.sleep(1)

def receive_clients():
    while not stop_event.is_set():
        try:
            data, address = sock.recvfrom(1024)
            if address not in clients:
                clients.add(address)
                print(f"New client: {address}")
        except OSError as e:
            if stop_event.is_set():
                break
            print(f"Error receiving data: {e}")

try:
    threading.Thread(target=handle_client, daemon=True).start()
    threading.Thread(target=receive_clients, daemon=True).start()
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    print("Server is shutting down...")
finally:
    stop_event.set()
    sock.close()
