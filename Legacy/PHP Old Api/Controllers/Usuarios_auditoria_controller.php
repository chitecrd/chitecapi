<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;

class Usuarios_auditoria_controller extends Controller
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
                    $usuarios_auditoria = DB::select("SELECT * FROM {$DB}.usuarios_auditoria");
                    $json->data = $usuarios_auditoria;
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
                    DB::insert("INSERT INTO {$DB}.usuarios_auditoria (nombre, apellido, usuario, `password`, estado, tipo_usuario, created_at, updated_at)
                        VALUES (?,?,?,?,?,?,?,?)", [
                        $datos_usuario['nombre'],
                        $datos_usuario['apellido'],
                        $datos_usuario['usuario'],
                        $datos_usuario['password'],
                        $datos_usuario['estado'],
                        $datos_usuario['tipo_usuario'],
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
}