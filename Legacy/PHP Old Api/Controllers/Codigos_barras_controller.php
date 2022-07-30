<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Codigos_barras_controller extends Controller
{
    public function get_codigos_barras(Request $request)
    {
        $json = new stdClass();
        $es_request_valido = validar_request(['DB'], $json, $request);
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
                $codigos_barras = DB::select("SELECT * FROM {$DB}.codigos_barras");
                $json->codigos_barras = $codigos_barras;
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

    public function importar_excel_codigos_barras(Request $request)
    {
        ini_set('memory_limit', '-1');
        ini_set('max_execution_time', '999999999');
        set_time_limit(999999999);
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

                //$secuencia = 1;
                if ($metodo_importacion == 1)
                {
                    DB::statement("TRUNCATE {$DB}.codigos_barras");
                }
                else
                {
                   // $secuencia = DB::select("SELECT (COUNT(1) + 1) as secuencia FROM {$DB}.codigos_barras");
                   // $secuencia = (count($secuencia) > 0) ? $secuencia[0]->secuencia : 1;
                }

                $spreadsheet = PHPExcel_IOFactory::load("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/{$nombre_excel}");
                $sheet = $spreadsheet->getSheet(0);
                $highestRow = $sheet->getHighestRow();
                $highestColumn = $sheet->getHighestColumn();
                date_default_timezone_set('UTC');
                for ($row = 1; $row <= $highestRow; $row++)
                {
                    $sheet->getStyle('A' . $row . ':' . $highestColumn . $row)->getNumberFormat()->setFormatCode("YYYY-MM-DD");
                    $datos_fila = $sheet->rangeToArray('A' . $row . ':' . $highestColumn . $row,
                                                    NULL,
                                                    TRUE,
                                                    FALSE);
                    if ($row >= 2)
                    {
                        $datos_fila = $datos_fila[0];
                        $no_articulo = ($datos_fila[0] != '') ? $datos_fila[0] : '';
                        $codigo_barra = ($datos_fila[1] != '') ? $datos_fila[1] : '';
                        DB::insert("INSERT INTO {$DB}.codigos_barras (no_articulo, codigo_barra,created_at,updated_at) VALUES (?,?,?,?)", [
                            $no_articulo,
                            $codigo_barra,
                            $fecha_hoy,
                            $fecha_hoy
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
