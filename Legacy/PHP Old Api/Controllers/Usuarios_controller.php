<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Usuarios_controller extends Controller
{
    public function index(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request(["DB"], $json, $request);
        if ($es_request_valido) 
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $usuarios = DB::select("SELECT u.*, 
                     (SELECT COUNT(1) FROM {$DB}.detalle_inventario d WHERE d.id_usuario_registro = u.id) as nro_tiros
                     FROM {$DB}.usuarios u");
                    foreach ($usuarios as $ind => $usuario) 
                    {
                        $usuarios[$ind]->password = $usuario->password;
                    }
                    $json->data = $usuarios;
                    $json->error = 0;
                    $json->error_type = 0;
                    $json->error_message = 0;    
                //}
            }
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }

        if ($json->error_type != 400) 
        {
            return response()->json(
                $json,
                200
            );
        } else 
        {
            throw new HttpException(400, $json->error_message);
        }
    }


    public function guardar(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass();
        if ($request != NULL && isset($_GET['DB']))
        {
            $DB = $_GET['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $datos_usuarios = $request;
                foreach ($datos_usuarios as $datos_usuario)
                {
                    DB::insert("INSERT INTO {$DB}.usuarios (nombre, apellido, usuario, `password`, estado, created_at, updated_at)
                        VALUES (?,?,?,?,?,?,?)", [
                        $datos_usuario['nombre'],
                        $datos_usuario['apellido'],
                        $datos_usuario['usuario'],
                        $datos_usuario['password'],
                        $datos_usuario['estado'],
                        $fecha_hoy,
                        $fecha_hoy
                    ]);
                }

                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0;
            }
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }
        else
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan parámetros';
        }

        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    public function editar(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass();
        if ($request != NULL && isset($_GET['DB']))
        {
            $DB = $_GET['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $datos_usuarios = $request;
                foreach ($datos_usuarios as $datos_usuario)
                {
                    DB::insert("UPDATE {$DB}.usuarios set nombre = ?, apellido = ?, usuario = ?, 
                     `password` = ?, estado = ?
                      WHERE id = ?", [
                        $datos_usuario['nombre'],
                        $datos_usuario['apellido'],
                        $datos_usuario['usuario'],
                        $datos_usuario['password'],
                        $datos_usuario['estado'],
                        $datos_usuario['id']
                    ]);
                }

                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0;
            }
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }
        else
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan parámetros';
        }

        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    public function importar_excel_usuarios(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request(['DB', 'nombre_excel', 'metodo_importacion'], $json, $request);
        if ($es_request_valido) 
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $nombre_excel = ($request->input('nombre_excel') != NULL) ? $request->input('nombre_excel') : '';
                $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : '';
                $row = 1;
                DB::raw("USE {$DB}");

                $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$DB} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$DB}_" .date('Y-m-d_H:i:s') .".sql";
                exec($cmd);

                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.usuarios");
                }

                $spreadsheet = PHPExcel_IOFactory::load("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_excel}");
                $sheet = $spreadsheet->getSheet(0);
                $highestRow = $sheet->getHighestRow(); 
                $highestColumn = $sheet->getHighestColumn();
                for ($row = 1; $row <= $highestRow; $row++)
                { 
                    $datos_fila = $sheet->rangeToArray('A' . $row . ':' . $highestColumn . $row,
                                                    NULL,
                                                    TRUE,
                                                    FALSE);
                    if ($row >= 2) 
                    {
                        $datos_fila = $datos_fila[0];
                        $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                        $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                        $datos_fila[4] = ($datos_fila[4] != '') ? $datos_fila[4] : 0;
                        DB::insert("INSERT INTO {$DB}.usuarios 
                         (nombre, apellido, usuario, password, estado)
                         VALUES (?,?,?,?,?)", $datos_fila);
                    }
                }
                
                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0; 
            }  
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }

        if ($json->error_type != 400) 
        {
            return response()->json(
                $json,
                200
            );
        } else 
        {
            throw new HttpException(400, $json->error_message);
        }
    }


    public function distribuir_auditores(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass();
        if (/*$request != NULL && */isset($_GET['DB']))
        {
            $DB = $_GET['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $valores = [];
                $condicion_id_tipo_ubicacion = "";
                if (isset($_GET['id_tipo_ubicacion'])) 
                {
                    $id_tipo_ubicacion = $_GET['id_tipo_ubicacion'];
                    $condicion_id_tipo_ubicacion = "AND u.id_tipo_ubicacion = ?";
                    $valores[] = $id_tipo_ubicacion;
                }
                $detalles_inventario = DB::select("SELECT d.*, ifnull(tu.descripcion, '') as tipo_ubicacion
                 FROM {$DB}.detalle_inventario d
                 left join {$DB}.ubicaciones u 
                   on (d.id_ubicacion = u.id)
                 left join {$DB}.tipo_ubicacion tu 
                  on (u.id_tipo_ubicacion = tu.id)
                 WHERE 1 = 1 {$condicion_id_tipo_ubicacion}", $valores);
                shuffle($detalles_inventario);

                $nro_usuarios = DB::select("SELECT COUNT(*) as nro_usuarios FROM {$DB}.usuarios_auditoria");
                $nro_usuarios = (count($nro_usuarios) > 0) ? $nro_usuarios[0]->nro_usuarios : 1;
               
                $cantidad_detalles_inventario = count($detalles_inventario);
                $cantidad_dividir = round($cantidad_detalles_inventario / $nro_usuarios);
                $cont_detalles_usuario = 0;
                $usuarios = DB::select("SELECT id FROM {$DB}.usuarios_auditoria");
                foreach ($usuarios as $ind => $usuario) 
                {
                    $detalles_inventario_usuario = [];
                    foreach ($detalles_inventario as $detalle_inventario) 
                    {
                        DB::update("UPDATE {$DB}.detalle_inventario set id_auditor = ? WHERE id = ?", [
                            $usuario->id,
                            $detalles_inventario[$cont_detalles_usuario]->id
                        ]);
                        $detalles_inventario_usuario[] = $detalles_inventario[$cont_detalles_usuario];
                        $cont_detalles_usuario++;
                        if ($cont_detalles_usuario == $cantidad_dividir) 
                        {
                            break;
                        } else if ($cont_detalles_usuario >= $cantidad_detalles_inventario) 
                        {
                            break;
                        }
                    }
                    $nombre_columna = "usuario{$usuario->id}";
                    $json->$nombre_columna = $detalles_inventario_usuario;
                }

                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0;
            }
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }
        else
        {
            $json->error = 1;
            $json->error_type = 400;
            $json->error_message = 'Faltan parámetros';
        }

        if ($json->error_type != 400)
        {
            return response()->json(
                $json,
                200
            );
        } else
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    public function importar_excel_usuarios_auditoria(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $json = new stdClass(); 
        $es_request_valido = validar_request(['DB', 'nombre_excel', 'metodo_importacion'], $json, $request);
        if ($es_request_valido) 
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $nombre_excel = ($request->input('nombre_excel') != NULL) ? $request->input('nombre_excel') : '';
                $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : '';
                $row = 1;
                DB::raw("USE {$DB}");

                $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$DB} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$DB}_" .date('Y-m-d_H:i:s') .".sql";
                exec($cmd);

                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.usuarios_auditoria");
                }

                $spreadsheet = PHPExcel_IOFactory::load("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_excel}");
                $sheet = $spreadsheet->getSheet(0);
                $highestRow = $sheet->getHighestRow(); 
                $highestColumn = $sheet->getHighestColumn();
                for ($row = 1; $row <= $highestRow; $row++)
                { 
                    $datos_fila = $sheet->rangeToArray('A' . $row . ':' . $highestColumn . $row,
                                                    NULL,
                                                    TRUE,
                                                    FALSE);
                    if ($row >= 2) 
                    {
                        $datos_fila = $datos_fila[0];
                        $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                        $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                        $datos_fila[4] = ($datos_fila[4] != '') ? $datos_fila[4] : 0;
                        $datos_fila[5] = ($datos_fila[5] != '') ? $datos_fila[5] : 0;
                        $datos_fila[6] = $fecha_hoy;
                        $datos_fila[7] = $fecha_hoy;
                        DB::insert("INSERT INTO {$DB}.usuarios_auditoria 
                         (nombre, apellido, usuario, password, estado, tipo_usuario, created_at, updated_at)
                         VALUES (?,?,?,?,?,?,?,?)", $datos_fila);
                    }
                }
                
                $json->error = 0;
                $json->error_type = 0;
                $json->error_message = 0; 
            }  
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }

        if ($json->error_type != 400) 
        {
            return response()->json(
                $json,
                200
            );
        } else 
        {
            throw new HttpException(400, $json->error_message);
        }
    }

    /*
    public function importar_csv_usuarios(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request(['DB', 'nombre_csv', 'metodo_importacion'], $json, $request);
        if ($es_request_valido) 
        {
            $DB = ($request->input('DB') != NULL) ? $request->input('DB') : '';
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $nombre_csv = ($request->input('nombre_csv') != NULL) ? $request->input('nombre_csv') : '';
                $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : '';
                $row = 1;
                DB::raw("USE {$DB}");

                $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$DB} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$DB}_" .date('Y-m-d_H:i:s') .".sql";
                exec($cmd);

                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.usuarios");
                }
                if (($handle = fopen("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_csv}", "r")) !== FALSE) {
                    while (($data = fgetcsv($handle, 1000, ",")) !== FALSE) 
                    {
                        $num = count($data);
                        $datos_fila = [];  
                        $row++;          
                        if ($row >= 3) 
                        {
                            for ($c=0; $c < $num; $c++) {
                                $datos_fila[] = $data[$c];
                            }

                            $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                            $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                            $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                            $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                            $datos_fila[4] = ($datos_fila[4] != '') ? $datos_fila[4] : 0;
                            DB::insert("INSERT INTO {$DB}.usuarios 
                             (nombre, apellido, usuario, password, estado)
                             VALUES (?,?,?,?,?)", $datos_fila);
                        }
                    }
                    fclose($handle);
    
                    $json->error = 0;
                    $json->error_type = 0;
                    $json->error_message = 0; 
                }
            }  
            else 
            {
                $json->error = 1;
                $json->error_type = 400;
                $json->error_message = "La base de datos: \"{$DB}\" no existe";
            } 
        }

        if ($json->error_type != 400) 
        {
            return response()->json(
                $json,
                200
            );
        } else 
        {
            throw new HttpException(400, $json->error_message);
        }
    }*/
}