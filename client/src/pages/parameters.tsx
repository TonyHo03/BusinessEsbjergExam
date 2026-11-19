import {useEffect, useState} from "react";
import * as React from "react";
import {ChevronDown, ChevronRight, Plus, Trash2} from "lucide-react";
import {api} from "@/api/client.ts";
import {type ParameterDto} from "@/api/Api.ts";
import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Button} from "@/components/ui/button.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "@/components/ui/select.tsx";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog.tsx";

const today = () => new Date().toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
const formatDate = (iso?: string | null) => iso ? new Date(iso).toLocaleDateString("da-DK") : "–";
const formatValue = (kind?: string | null, value?: string | null) =>
    value == null ? "–" : kind === "Boolean" ? (value === "true" ? "On" : "Off") : value;

const statusStyles: Record<string, string> = {
    Current: "bg-success-bg text-success-fg",
    Upcoming: "bg-[#CE6B4C]/15 text-[#B5583B]",
    Past: "bg-muted text-muted-foreground",
};

export function ParametersView() {
    const [parameters, setParameters] = useState<ParameterDto[]>([]);
    const [expanded, setExpanded] = useState<string | null>(null);
    const [dialogKey, setDialogKey] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const load = () => api.api.parametersGet({}).then(setParameters).catch(() => setError("Could not load the parameters."));

    useEffect(() => { load(); }, []);

    const handleDelete = async (id: string) => {
        try {
            await api.api.parametersDelete({id});
            await load();
        } catch (err: any) {
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.detail ?? "Could not delete the value.");
        }
    };

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full">
                <Label className="text-[#0F2338] text-3xl font-normal">Parameters</Label>
            </div>
            <Label className="text-[#516080]">
                Changes apply from a start date and never rewrite the past. Earlier values still apply to dates before.
            </Label>
            <Separator className="h-px"/>

            {error && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{error}</p>
            )}

            <div className="border rounded-xl bg-card">
                <Table>
                    <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                        <TableRow>
                            <TableHead className="w-8"/>
                            <TableHead>Parameter</TableHead>
                            <TableHead>Current value</TableHead>
                            <TableHead>Applies from</TableHead>
                            <TableHead>Upcoming</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                        {parameters.map(p => {
                            const open = expanded === p.key;
                            const upcoming = p.versions?.filter(v => v.status === "Upcoming") ?? [];
                            return (
                                <React.Fragment key={p.key}>
                                    <TableRow>
                                        <TableCell>
                                            <button type="button" aria-expanded={open} aria-label={`Show history for ${p.description}`}
                                                    onClick={() => setExpanded(open ? null : p.key!)}>
                                                {open ? <ChevronDown className="size-4"/> : <ChevronRight className="size-4"/>}
                                            </button>
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex flex-col">
                                                <span className="font-medium">{p.description}</span>
                                                <span className="text-xs text-muted-foreground">{p.key}</span>
                                            </div>
                                        </TableCell>
                                        <TableCell className="font-semibold">{formatValue(p.kind, p.currentValue)}</TableCell>
                                        <TableCell>{formatDate(p.currentFrom)}</TableCell>
                                        <TableCell>
                                            {upcoming.length > 0
                                                ? upcoming.map(v => <div key={v.id}>{formatValue(p.kind, v.value)} from {formatDate(v.effectiveFrom)}</div>)
                                                : <span className="text-muted-foreground">–</span>}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <button type="button" onClick={() => setDialogKey(p.key!)}
                                                    className="inline-flex h-9 items-center gap-2 rounded-lg border border-[#E9E1D6] bg-card px-4 text-sm font-medium text-[#023459] hover:bg-muted">
                                                <Plus className="size-4"/> New value
                                            </button>
                                        </TableCell>
                                    </TableRow>
                                    {open && (
                                        <TableRow className="bg-muted/40 hover:bg-muted/40">
                                            <TableCell/>
                                            <TableCell colSpan={5}>
                                                <div className="flex flex-col gap-2">
                                                    <span className="text-sm font-medium">History</span>
                                                    {p.versions?.map(v => (
                                                        <div key={v.id} className="flex items-center gap-4 text-sm">
                                                            <span className="w-24 font-semibold">{formatValue(p.kind, v.value)}</span>
                                                            <span className="w-32">from {formatDate(v.effectiveFrom)}</span>
                                                            <Badge className={statusStyles[v.status ?? "Past"]}>{v.status}</Badge>
                                                            {v.status === "Upcoming" && (
                                                                <button type="button" onClick={() => handleDelete(v.id!)}
                                                                        className="inline-flex items-center gap-1 text-[#B5583B] hover:underline">
                                                                    <Trash2 className="size-4"/> Delete
                                                                </button>
                                                            )}
                                                        </div>
                                                    ))}
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </React.Fragment>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>

            <NewValueDialog parameters={parameters} parameterKey={dialogKey}
                            onClose={() => setDialogKey(null)} onSaved={() => { setDialogKey(null); load(); }}/>
        </div>
    );
}

function NewValueDialog({parameters, parameterKey, onClose, onSaved}: {
    parameters: ParameterDto[],
    parameterKey: string | null,
    onClose: () => void,
    onSaved: () => void
}) {
    const [key, setKey] = useState<string>("");
    const [value, setValue] = useState<string>("");
    const [effectiveFrom, setEffectiveFrom] = useState<string>(today());
    const [errors, setErrors] = useState<string[]>([]);
    const [submitting, setSubmitting] = useState(false);

    const selected = parameters.find(p => p.key === key);

    useEffect(() => {
        if (parameterKey) {
            setKey(parameterKey);
            setValue(parameters.find(p => p.key === parameterKey)?.currentValue ?? "");
            setEffectiveFrom(today());
            setErrors([]);
        }
    }, [parameterKey]);

    // Når man skifter parameter i dialogen, foreslås dens nuværende værdi
    const handleKeyChange = (newKey: string) => {
        setKey(newKey);
        setValue(parameters.find(p => p.key === newKey)?.currentValue ?? "");
        setErrors([]);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setErrors([]);
        try {
            await api.api.parametersAdd({key, value, effectiveFrom});
            onSaved();
        } catch (err: any) {
            // API'et sender en liste af fejl ved 400
            const body = await err?.json?.().catch(() => null);
            setErrors(Array.isArray(body) ? body : ["Could not save the value."]);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Dialog open={parameterKey !== null} onOpenChange={open => !open && onClose()}>
            <DialogContent className="sm:max-w-md border-2">
                <DialogHeader>
                    <DialogTitle>New value</DialogTitle>
                    <DialogDescription>The current value keeps applying to dates before the start date.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-2">
                        <Label>Parameter</Label>
                        <Select value={key} onValueChange={v => v && handleKeyChange(v as string)}
                                items={parameters.map(p => ({value: p.key!, label: p.description!}))}>
                            <SelectTrigger className="w-full"><SelectValue/></SelectTrigger>
                            <SelectContent>
                                {parameters.map(p => <SelectItem key={p.key} value={p.key!}>{p.description}</SelectItem>)}
                            </SelectContent>
                        </Select>
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="value">Value</Label>
                        {selected?.kind === "Boolean" ? (
                            <Select value={value} onValueChange={v => v && setValue(v as string)}
                                    items={[{value: "true", label: "On"}, {value: "false", label: "Off"}]}>
                                <SelectTrigger id="value" className="w-full"><SelectValue/></SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="true">On</SelectItem>
                                    <SelectItem value="false">Off</SelectItem>
                                </SelectContent>
                            </Select>
                        ) : (
                            <Input id="value" inputMode="decimal" value={value} onChange={e => setValue(e.target.value)} required/>
                        )}
                        {selected?.kind === "Decimal" && <span className="text-xs text-muted-foreground">Use a dot as decimal separator, e.g. 2.08</span>}
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="effectiveFrom">Applies from</Label>
                        <Input id="effectiveFrom" type="date" min={today()} value={effectiveFrom}
                               onChange={e => setEffectiveFrom(e.target.value)} required/>
                    </div>

                    {errors.length > 0 && (
                        <ul role="alert" className="text-sm text-destructive list-disc pl-5">
                            {errors.map(e => <li key={e}>{e}</li>)}
                        </ul>
                    )}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Cancel</Button>}/>
                        <Button type="submit" disabled={submitting}>Save</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}