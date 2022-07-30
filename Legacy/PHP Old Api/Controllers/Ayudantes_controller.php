<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Ayudantes_controller extends Controller
{
    public function index(Request $request) 
    {
        $json = new stdClass(); 
        $es_request_valido = validar_request([/*'token',*/ 'DB'], $json, $request);
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
                $ayudantes = DB::select("SELECT * FROM {$DB}.ayudantes");
                $json->data = $ayudantes;
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
                $datos_ayudantes = $request;
                foreach ($datos_ayudantes as $datos_ayudante)
                {
                    try 
                    {
                        $nuevo_autoincrement = DB::select("SELECT COUNT(1) as nuevo_autoincrement FROM {$DB}.ayudantes");
                        $nuevo_autoincrement = (count($nuevo_autoincrement) > 0) ? ($nuevo_autoincrement[0]->nuevo_autoincrement + 1) : 1;
                        DB::connection()->getpdo()->exec("ALTER TABLE {$DB}.ayudantes AUTO_INCREMENT = {$nuevo_autoincrement}");
                        DB::insert("INSERT INTO {$DB}.ayudantes (codigo_unico, nombre, created_at, updated_at)
                            VALUES (?,?,?,?)", [
                            $datos_ayudante['codigo_unico'],
                            $datos_ayudante['nombre'],
                            $fecha_hoy,
                            $fecha_hoy
                        ]);
                    } catch (\Exception $ex) 
                    {
                        DB::update("UPDATE {$DB}.ayudantes set nombre = ?, 
                            updated_at = ?
                            WHERE codigo_unico = ?", [
                            $datos_ayudante['nombre'],
                            $fecha_hoy,
                            $datos_ayudante['codigo_unico']
                        ]);
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

    public function importar_excel_ayudantes(Request $request) 
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

                $secuencia = 1;
                if ($metodo_importacion == 1) 
                {
                    DB::statement("TRUNCATE {$DB}.ayudantes");
                }
                else 
                {
                    $secuencia = DB::select("SELECT (COUNT(1) + 1) as secuencia FROM {$DB}.ayudantes");
                    $secuencia = (count($secuencia) > 0) ? $secuencia[0]->secuencia : 1;
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
                        $valores = [];
                        $valores[] = str_pad(($secuencia), 4, '0', STR_PAD_LEFT);
                        $valores[] = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $valores[] = $fecha_hoy;
                        $valores[] = $fecha_hoy;
                        DB::insert("INSERT INTO {$DB}.ayudantes 
                         (codigo_unico, nombre, created_at, updated_at)
                         VALUES (?,?,?,?)", $valores);
                        $secuencia++;
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
}