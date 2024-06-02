import socket
import threading
import time

# Global variables
player_id_counter = 1
player_connections = {}
player_ready_status = {}
player_scores = {}
player_positions = {}
player_alive_status = {}  # 记录玩家是否存活
player_count = 0
player_alive_count = 99
game_running = False
available_ids = []

def get_local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(('10.254.254.254', 1))
        local_ip = s.getsockname()[0]
    except Exception:
        local_ip = '127.0.0.1'
    finally:
        s.close()
    return local_ip

def broadcast(message):
    for conn in player_connections.values():
        conn.send((message + "\n").encode())

def broadcast_player_count():
    global player_count
    message = f"PlayerCount/{player_count}"
    broadcast(message)

def broadcast_start_game():
    global game_running, player_alive_count
    message = "StartGame"
    broadcast(message)
    game_running = True
    player_alive_count = player_count
    if player_alive_count == 1:
        player_alive_count = 2
    threading.Thread(target=update_player_positions).start()

def broadcast_ready_count():
    ready_count = sum(player_ready_status.values())
    message = f"ReadyCount/{ready_count}"
    broadcast(message)

def check_all_ready():
    broadcast_ready_count()
    if all(player_ready_status.values()) and player_ready_status:
        broadcast_start_game()
        time.sleep(2)

def handle_client_connection(connection, address):
    global player_id_counter, player_count, player_ready_status, player_scores, player_positions, player_alive_status, available_ids, player_alive_count, game_running
    
    if available_ids:
        player_id = available_ids.pop(0)
    else:
        player_id = player_id_counter
        player_id_counter += 1

    player_connections[player_id] = connection
    player_ready_status[player_id] = False
    player_scores[player_id] = 0
    player_positions[player_id] = (-10.0, -10.0, -10.0)  # Initial position
    player_alive_status[player_id] = True  # 玩家初始状态为存活
    player_count += 1

    print(f"New connection from {address}. Assigned player ID: {player_id}")
    connection.send((str(player_id) + "\n").encode())
    time.sleep(0.1)
    broadcast_player_count()

    while True:
        try:
            data = connection.recv(1024).decode().strip()
            if not data:
                print(f"Player {player_id} disconnected.")
                break

            print(f"Received from player {player_id}: {data}")

            if data == f"{player_id}/Ready":
                player_ready_status[player_id] = True
                print(f"Player {player_id} is ready.")
                check_all_ready()
            elif data.startswith(f"{player_id}/Bomb/"):
                _, _, x, y, size = data.split('/')
                broadcast(f"Bomb/{player_id}/{x}/{y}/{size}")
            elif data == f"{player_id}/CancelReady":
                player_ready_status[player_id] = False
                print(f"Player {player_id} cancelled readiness.")
                broadcast_ready_count()
            elif data.startswith(f"{player_id}/Hit/"):
                _, _, hit_id = data.split('/')
                hit_id = int(hit_id)
                player_scores[player_id] += 1
                broadcast(f"Remove/{hit_id}")
            elif data.startswith(f"{player_id}/Score/"):
                _, _, score = data.split('/')
                score = int(score)
                player_scores[player_id] = score
            elif data.startswith(f"{player_id}/Position/"):
                parts = data.split('/')
                if len(parts) == 5 and player_alive_status[player_id]:
                    _, _, x, z, rotation_y = parts
                    player_positions[player_id] = (float(x), float(z), float(rotation_y))
            elif data == f"{player_id}/Dead":
                player_alive_status[player_id] = False
                player_alive_count -= 1
                if player_alive_count <= 1:
                    game_running = False
                    broadcast('EndGame')
                broadcast(f"Remove/{player_id}")

            elif data.startswith("Positions/"):
                parts = data.split('/')
                for i in range(1, len(parts), 4):
                    id = int(parts[i])
                    x = float(parts[i+1])
                    z = float(parts[i+2])
                    rotation_y = float(parts[i+3])
                    if player_alive_status[id]:
                        player_positions[id] = (x, z, rotation_y)
            elif data.startswith(f"{player_id}/EndGame"):
                game_running = False

        except (socket.error, ConnectionResetError) as e:
            print(f"Error with player {player_id}: {e}")
            break

    connection.close()
    del player_connections[player_id]
    del player_ready_status[player_id]
    del player_scores[player_id]
    del player_positions[player_id]
    del player_alive_status[player_id]
    player_count -= 1
    available_ids.append(player_id)
    available_ids.sort()
    broadcast_player_count()
    print(f"Player {player_id} connection closed.")

def update_player_positions():
    global game_running
    while game_running:
        if player_positions:
            positions = "Positions"
            for player_id, (x, z, rotation_y) in player_positions.items():
                if player_alive_status[player_id]:  # 只发送存活玩家的位置
                    positions += f"/{player_id}/{x:.2f}/{z:.2f}/{rotation_y:.2f}"
            broadcast(positions)
        time.sleep(0.1)

def main():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind(('0.0.0.0', 8888))
    server_socket.listen(5)

    local_ip = get_local_ip()
    print(f"Server is listening on {local_ip}:8888")
    print("Server is listening for incoming connections...")

    try:
        while True:
            client_connection, client_address = server_socket.accept()
            print(f"Accepted new connection from {client_address}")

            client_thread = threading.Thread(target=handle_client_connection,
                                             args=(client_connection, client_address))
            client_thread.start()
    except KeyboardInterrupt:
        print("Server is shutting down...")
    finally:
        server_socket.close()
        print("Server socket closed.")

if __name__ == '__main__':
    main()
