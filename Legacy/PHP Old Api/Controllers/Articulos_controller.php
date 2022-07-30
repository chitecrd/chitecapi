<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use Schema;
use App\Articulos;
use PHPExcel;
use PHPExcel_IOFactory;
class Articulos_controller extends Controller
{
    public function index(Request $request) 
    {
        ini_set('memory_limit', '-1');  
        set_time_limit(300);
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $articulos = DB::select("SELECT * FROM {$DB}.articulos");
                    $json->data = $articulos;
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

    public function importar_excel_articulos(Request $request) 
    {
        ini_set('memory_limit', '-1');  
        set_time_limit(300);
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
                    DB::statement("TRUNCATE {$DB}.articulos");
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
                        $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : 0;
                        $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                        $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                        $datos_fila[4] = ($datos_fila[4] != '') ? $datos_fila[4] : '';
                        $datos_fila[5] = ($datos_fila[5] != '') ? $datos_fila[5] : '';
                        $datos_fila[6] = ($datos_fila[6] != '') ? $datos_fila[6] : 0;
                        $datos_fila[7] = ($datos_fila[7] != '') ? $datos_fila[7] : '';
                        $datos_fila[8] = ($datos_fila[8] != '') ? $datos_fila[8] : 0;
                        $datos_fila[9] = ($datos_fila[9] != '') ? $datos_fila[9] : 0;
                        $datos_fila[10] = (isset($datos_fila[10]) && $datos_fila[10] != '') ? $datos_fila[10] : '';
                        $datos_fila[11] = (isset($datos_fila[11]) && $datos_fila[11] != '') ? $datos_fila[11] : '';
                        $datos_fila[12] = (isset($datos_fila[12]) && $datos_fila[12] != '') ? $datos_fila[12] : '';
                        DB::insert("INSERT INTO {$DB}.articulos 
                         (no_articulo, codigo_barra, alterno1, alterno2, alterno3, descripcion, existencia,
                          unidad_medida, costo, precio, referencia, marca, familia)
                         VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?)", $datos_fila);
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
    public function importar_csv_articulos(Request $request) 
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
                    DB::statement("TRUNCATE {$DB}.articulos");
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

                            $datos_fila[0] = ($datos_fila[0] != '') ? $datos_fila[0] : 0;
                            $datos_fila[1] = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                            $datos_fila[2] = ($datos_fila[2] != '') ? $datos_fila[2] : '';
                            $datos_fila[3] = ($datos_fila[3] != '') ? $datos_fila[3] : '';
                            $datos_fila[4] = ($datos_fila[4] != '') ? $datos_fila[4] : '';
                            $datos_fila[5] = ($datos_fila[5] != '') ? $datos_fila[5] : '';
                            $datos_fila[6] = ($datos_fila[6] != '') ? $datos_fila[6] : 0;
                            $datos_fila[7] = ($datos_fila[7] != '') ? $datos_fila[7] : '';
                            $datos_fila[8] = ($datos_fila[8] != '') ? $datos_fila[8] : 0;
                            $datos_fila[9] = ($datos_fila[9] != '') ? $datos_fila[9] : 0;
                            DB::insert("INSERT INTO {$DB}.articulos 
                             (no_articulo, codigo_barra, alterno1, alterno2, alterno3, descripcion, existencia,
                              unidad_medida, costo, precio)
                             VALUES (?,?,?,?,?,?,?,?,?,?)", $datos_fila);
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

    public function get_nro_articulos(Request $request) 
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $nro_articulos = DB::select("SELECT COUNT(1) as nro_articulos FROM {$DB}.articulos");
                    $json->nro_articulos = $nro_articulos[0]->nro_articulos;
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

    public function get_costo_total_articulos(Request $request) 
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $costo_total = DB::select("SELECT SUM(costo * IFNULL(existencia, 0)) as costo_total FROM {$DB}.articulos");
                    $json->costo_total = $costo_total[0]->costo_total;
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

    public function get_precio_total_articulos(Request $request) 
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $precio_total = DB::select("SELECT SUM(precio * IFNULL(existencia, 0)) as precio_total FROM {$DB}.articulos");
                    $json->precio_total = $precio_total[0]->precio_total;
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
                $datos_articulos = $request;
                foreach ($datos_articulos as $datos_articulo)
                {
                    $referencia = isset($datos_articulo['referencia']) ? $datos_articulo['referencia'] : "";
                    $marca = isset($datos_articulo['marca']) ? $datos_articulo['marca'] : "";
                    $familia = isset($datos_articulo['familia']) ? $datos_articulo['familia'] : '';
                    DB::insert("INSERT INTO {$DB}.articulos (no_articulo, 
                        codigo_barra, alterno1, alterno2, alterno3, descripcion, existencia,
                        unidad_medida, costo, precio, referencia, marca, familia, 
                        created_at, updated_at)
                        VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", [
                        $datos_articulo['no_articulo'],
                        $datos_articulo['codigo_barra'],
                        $datos_articulo['alterno1'],
                        $datos_articulo['alterno2'],
                        $datos_articulo['alterno3'],
                        $datos_articulo['descripcion'],
                        $datos_articulo['existencia'],
                        $datos_articulo['unidad_medida'],
                        $datos_articulo['costo'],
                        $datos_articulo['precio'],
                        $referencia,
                        $marca,
                        $familia,
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
                $datos_articulos = $request;
                foreach ($datos_articulos as $datos_articulo)
                {
                    $valores = [
                        $datos_articulo['codigo_barra'],
                        $datos_articulo['alterno1'],
                        $datos_articulo['alterno2'],
                        $datos_articulo['alterno3'],
                        $datos_articulo['descripcion'],
                        $datos_articulo['existencia'],
                        $datos_articulo['unidad_medida'],
                        $datos_articulo['costo'],
                        $datos_articulo['precio']
                    ];
                    $columna_referencia = "";
                    if (isset($datos_articulo['referencia'])) 
                    {
                        $columna_referencia = ",referencia = ?";
                        $valores[] = $datos_articulo['referencia'];
                    } 

                    $columna_marca = "";
                    if (isset($datos_articulo['marca'])) 
                    {
                        $columna_marca = ",marca = ?";
                        $valores[] = $datos_articulo['marca'];
                    } 

                    $columna_familia = "";
                    if (isset($datos_articulo['familia'])) 
                    {
                        $columna_familia = ",familia = ?";
                        $valores[] = $datos_articulo['familia'];
                    } 

                    $valores[] = $datos_articulo['no_articulo'];
                    DB::insert("UPDATE {$DB}.articulos set codigo_barra = ?, alterno1 = ?, alterno2 = ?, alterno3 = ?, descripcion = ?, 
                        existencia = ?, unidad_medida = ?, costo = ?, precio = ? {$columna_referencia} {$columna_marca}
                        {$columna_familia}
                        WHERE no_articulo = ?", $valores);
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
}