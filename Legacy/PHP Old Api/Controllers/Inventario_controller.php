<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use App\Inventario;
use DB;

class Inventario_controller extends Controller
{
    public function guardar(Request $request) 
    {
        $fecha_hoy = date('Y-m-d H:i:s');
        $request = json_decode(file_get_contents('php://input'), true);
        $json = new stdClass();
        if ($request != NULL && isset($request['inventarios']) && isset($request['DB']))
        {
            $DB = $request['DB'];
            $existe_base_datos = DB::select('SELECT 1
             FROM INFORMATION_SCHEMA.SCHEMATA
             WHERE SCHEMA_NAME = ?
             LIMIT 1', [
                $DB
            ]);
            $existe_base_datos = (count($existe_base_datos) > 0);
            if ($existe_base_datos) 
            {
                $datos_inventarios = ($request['inventarios'] != NULL) ? $request['inventarios'] : [];
                foreach ($datos_inventarios as $datos_inventario)
                {
                    try
                    {
                        $nro_inventarios = DB::select("SELECT COUNT(*) as nro_inventarios FROM {$DB}.inventario");
                        $nro_inventarios = $nro_inventarios[0]->nro_inventarios;
                        DB::update("ALTER TABLE {$DB}.inventario AUTO_INCREMENT = {$nro_inventarios}");

                        DB::insert("INSERT INTO {$DB}.inventario (id_terminal, no_inventario, fecha_inicio, 
                         fecha_fin, estado, id_usuario_inicio, id_usuario_fin, created_at, updated_at)
                         VALUES (?,?,?,?,?,?,?,?,?)", [
                           $datos_inventario['id_terminal'],
                           $datos_inventario['no_inventario'],
                           $datos_inventario['fecha_inicio'],
                           $datos_inventario['fecha_fin'],
                           $datos_inventario['estado'],
                           $datos_inventario['id_usuario_inicio'],
                           $datos_inventario['id_usuario_fin'],
                           $fecha_hoy,
                           $fecha_hoy
                        ]);
                    } catch (\Exception $e) {
                        $es_actualizado = DB::update("UPDATE {$DB}.inventario set id_terminal = ?,
                        fecha_inicio = ?, fecha_fin = ?, estado = ?,
                        id_usuario_inicio = ?, id_usuario_fin = ?
                        WHERE no_inventario = ?", [
                            $datos_inventario['id_terminal'],
                            $datos_inventario['fecha_inicio'],
                            $datos_inventario['fecha_fin'],
                            $datos_inventario['estado'],
                            $datos_inventario['id_usuario_inicio'],
                            $datos_inventario['id_usuario_fin'],
                            $datos_inventario['no_inventario']
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
}