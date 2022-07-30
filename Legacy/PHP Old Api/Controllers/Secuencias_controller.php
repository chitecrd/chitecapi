<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Secuencias_controller extends Controller
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
                //Validamos el token
                //if (validar_token($request->input('token'), $json, $DB)) 
                //{
                    $secuencia_detalle_inventario = DB::select("SELECT COUNT(1) as secuencia_detalle_inventario
                     FROM {$DB}.detalle_inventario");
                    $secuencia_detalle_inventario = (count($secuencia_detalle_inventario) > 0) ? $secuencia_detalle_inventario[0]->secuencia_detalle_inventario : 0;

                    $secuencia_ayudantes = DB::select("SELECT COUNT(1) as secuencia_ayudantes
                     FROM {$DB}.ayudantes");
                    $secuencia_ayudantes = (count($secuencia_ayudantes) > 0) ? $secuencia_ayudantes[0]->secuencia_ayudantes : 0;

                    $secuencia_asignaciones = DB::select("SELECT COUNT(1) as secuencia_asignaciones
                     FROM {$DB}.asignaciones");
                    $secuencia_asignaciones = (count($secuencia_asignaciones) > 0) ? $secuencia_asignaciones[0]->secuencia_asignaciones : 0;

                    $json->secuencia_detalle_inventario = $secuencia_detalle_inventario;
                    $json->secuencia_ayudantes = $secuencia_ayudantes;
                    $json->secuencia_asignaciones = $secuencia_asignaciones;
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
}