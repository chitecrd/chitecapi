<?php
namespace App\Http\Controllers;

use Illuminate\Http\Request;
use stdClass;
use Symfony\Component\HttpKernel\Exception\HttpException;
use DB;
use PHPExcel;
use PHPExcel_IOFactory;
class Importar_controller extends Controller
{
    public function importar_excel_articulos(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_articulos?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_articulos_importado=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_ubicaciones(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_ubicaciones?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);

            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_ubicaciones_importado=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }
    public function importar_excel_usuarios(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_usuarios?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_usuarios_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_asignaciones(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_asignaciones?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_asignaciones_importadas=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_usuarios_auditoria(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_usuarios_auditoria?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_usuarios_auditoria_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_ayudantes(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_ayudantes?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_ayudantes_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_detalle_inventario_ciclico(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_detalle_inventario_ciclico?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);

            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_detalles_inventarios_ciclicos_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_familias_productos(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_familias_productos?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_familias_productos_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_detalle_inventario(Request $request)
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_detalle_inventario?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_detalle_inventario_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function importar_excel_codigos_barras(Request $request) 
    {
        if ($request->hasFile('excel_importar'))
        {
            $nombre_base_datos = ($request->input('nombre_base_datos') != NULL) ? $request->input('nombre_base_datos') : 1;
            $metodo_importacion = ($request->input('metodo_importacion') != NULL) ? $request->input('metodo_importacion') : 1;
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            $ch = curl_init();
            curl_setopt($ch, CURLOPT_URL,"http://165.227.213.74/wmsmobile/api/importar_excel_codigos_barras?DB={$nombre_base_datos}");
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "nombre_excel={$nombre_excel}&metodo_importacion={$metodo_importacion}");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
            $respuesta = curl_exec($ch);
            curl_close ($ch);
            if (json_decode($respuesta) != NULL && json_decode($respuesta)->error == 0)
            {
                return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_codigo_barras_importados=1');
            }
            else
            {
                $mensaje_error = (strpos($respuesta, 'La base de datos:') != FALSE) ? "La base de datos: \"{$nombre_base_datos}\" no existe"
                : "";
                return redirect("importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?error={$mensaje_error}");
            }
        }
    }

    public function actualizar_costos_articulos(Request $request) 
    {
        ini_set('memory_limit', '-1');
        ini_set('max_execution_time', '999999999');
        set_time_limit(999999999);
        if ($request->hasFile('excel_importar') && $request->exists('nombre_base_datos'))
        {
            $nombre_base_datos = $request->input('nombre_base_datos');
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            DB::raw("USE {$nombre_base_datos}");
            $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$nombre_base_datos} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$nombre_base_datos}_" .date('Y-m-d_H:i:s') .".sql";
            exec($cmd);

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
                    $nuevo_costo = ($datos_fila[1] != '') ? $datos_fila[1] : '';

                    DB::update("UPDATE {$nombre_base_datos}.articulos set costo = ? WHERE no_articulo = ?", [
                        $nuevo_costo,
                        $no_articulo
                    ]);
                    return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_costo_articulos_actualizados=1');
                }
            }
        }
    }

    public function actualizar_costos_detalle_inventario(Request $request) 
    {
        ini_set('memory_limit', '-1');
        ini_set('max_execution_time', '999999999');
        set_time_limit(999999999);
        if ($request->hasFile('excel_importar') && $request->exists('nombre_base_datos'))
        {
            $nombre_base_datos = $request->input('nombre_base_datos');
            $nombre_excel = $request->file('excel_importar')->getClientOriginalName();
            $nombre_excel = str_replace(' ', '_', $nombre_excel);
            $request->file('excel_importar')->move("{$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/",$nombre_excel);

            DB::raw("USE {$nombre_base_datos}");
            $cmd = "mysqldump -h localhost -u admin -pReemlasr2019** {$nombre_base_datos} > {$_SERVER['DOCUMENT_ROOT']}/wmsmobile/public/archivos/importar/backups/backup_{$nombre_base_datos}_" .date('Y-m-d_H:i:s') .".sql";
            exec($cmd);

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
                    $nuevo_costo = ($datos_fila[1] != '') ? $datos_fila[1] : '';

                    DB::update("UPDATE {$nombre_base_datos}.detalle_inventario set costo = ? WHERE no_articulo = ?", [
                        $nuevo_costo,
                        $no_articulo
                    ]);
                    return redirect('importar/yGSwz8sdq4@Q6rQGgjQqyGSwz8sdq4@Q6rQGgjQq?es_costo_detalles_actualizados=1');
                }
            }
        }
    }
}
