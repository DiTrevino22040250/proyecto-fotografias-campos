#!/bin/bash
echo "Esperando que los masters estén listos..."
sleep 30

# Obtener posición del master DB1
DB1_STATUS=$(mysql -h db1 -u root -prootpassword -e "SHOW MASTER STATUS\G" 2>/dev/null)
DB1_FILE=$(echo "$DB1_STATUS" | grep "File:" | awk '{print $2}')
DB1_POS=$(echo "$DB1_STATUS" | grep "Position:" | awk '{print $2}')

echo "Configurando replicación desde DB1..."
mysql -h db_replica -u root -prootpassword <<EOF
STOP SLAVE;
CHANGE REPLICATION SOURCE TO
    SOURCE_HOST='db1',
    SOURCE_USER='replica_user',
    SOURCE_PASSWORD='replica_password',
    SOURCE_LOG_FILE='$DB1_FILE',
    SOURCE_LOG_POS=$DB1_POS,
    SOURCE_AUTO_POSITION=0;
START SLAVE;
EOF
echo "Replicación configurada correctamente"
