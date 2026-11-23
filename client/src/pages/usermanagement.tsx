import {Label} from "@/components/ui/label.tsx";
import {Ban, CalendarClock, Edit, MoreHorizontal, UserPlus} from "lucide-react";
import {Separator} from "@/components/ui/separator.tsx";
import {Card, CardContent} from "../components/ui/card.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "../components/ui/select.tsx";
import {type EmployeeDto, Roles, Status} from "@/api/Api.ts";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "../components/ui/table.tsx";
import {Avatar, AvatarFallback} from "@/components/ui/avatar.tsx";
import {initials} from "../components/nav-user.tsx";
import {Badge} from "../components/ui/badge.tsx";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger
} from "@/components/ui/dropdown-menu.tsx";
import * as React from "react";
import {useEffect, useState} from "react";
import {api} from "@/api/client.ts";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog.tsx";
import {Button} from "@/components/ui/button.tsx";
import {WorkScheduleDialog} from "@/components/WorkScheduleDialog.tsx";


export function UserManagementView() {


    const [employees, setEmployees] = useState<EmployeeDto[]>([])
    const [search, setSearch] = useState<string>("");
    const [roleFilter, setRoleFilter] = useState<Roles | null>(null);
    const [open, setOpen] = useState<boolean>(false);
    const [openEditing, setOpenEditing] = useState<boolean>(false);

    const [editingEmployee, setEditingEmployee] = useState<EmployeeDto | null>(null);
    const [scheduleEmployee, setScheduleEmployee] = useState<EmployeeDto | null>(null);


    const loadEmployees = () => api.api.userGetAllUsers().then(setEmployees).catch(err => console.log(err));

    useEffect(() => { loadEmployees(); }, []);

    // Søg på navn eller email (uden forskel på store og små bogstaver) og filtrér på rolle
    const term = search.trim().toLowerCase();
    const filtered = employees.filter(e =>
        (!term || e.name?.toLowerCase().includes(term) || e.email?.toLowerCase().includes(term)) &&
        (!roleFilter || e.role === roleFilter));

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full">
                <Label className="text-[#0F2338] text-3xl font-normal">User Management</Label>
                <button onClick={() => setOpen(true)} className="flex h-9 justify-center items-center bg-primary text-sm font-medium px-6 rounded-lg gap-4 ml-auto text-primary-foreground"><UserPlus/> Add employee</button>
            </div>
            <Label className="text-[#516080]">Manage users and assign roles and permissions.</Label>
            <Separator className="h-px"/>
            <div className="flex w-full gap-4">
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Active users</p>
                        <p className="text-3xl font-semibold text-[#023459]">{employees.filter(e => e.status === Status.Active).length}</p>
                        <p className="text-xs text-muted-foreground">Permissions: Own balances and requests</p>
                    </CardContent>
                </Card>
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Accounting access</p>
                        <p className="text-3xl font-semibold text-[#023459]">{employees.filter(e => e.role === Roles.Accountant).length}</p>
                        <p className="text-xs text-muted-foreground">Permissions: All employee access plus approvals and company balances</p>
                    </CardContent>
                </Card>
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Administrators</p>
                        <p className="text-3xl font-semibold text-[#023459]">{employees.filter(e => e.role === Roles.Admin).length}</p>
                        <p className="text-xs text-muted-foreground">Permissions: All accounting permissions plus policy and user management</p>
                    </CardContent>
                </Card>
            </div>
            <Separator className="h-px"/>
            <div className="flex w-full">
                <div className="flex flex-col gap-2">
                    <Label htmlFor="search">Find a user:</Label>
                    <Input id="search" className="bg-card! w-72" placeholder="Search by name or email..."
                           value={search} onChange={e => setSearch(e.target.value)}/>
                </div>
                <div className="flex flex-col gap-2 ml-auto">
                    <Label>Role:</Label>
                    <Select value={roleFilter} onValueChange={v => setRoleFilter((v as Roles) ?? null)}>
                        <SelectTrigger>
                            <SelectValue className="pr-12" placeholder={"All Roles"}/>
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value={null}>All Roles</SelectItem>
                            {Object.values(Roles).map(r => <SelectItem key={r} value={r}>{r}</SelectItem>)}
                        </SelectContent>
                    </Select>
                </div>
            </div>
            <UserTable employees={filtered} setEditingEmployee={setEditingEmployee} setOpenEditing={setOpenEditing} setScheduleEmployee={setScheduleEmployee}/>
            {/* En ny medarbejder skal have et arbejdsskema, så skemadialogen åbner med det samme. Rollekontiene har ingen ferie og intet skema */}
            <CreateEmployeeDialog open={open} setOpen={setOpen}
                                  onCreated={created => { loadEmployees(); if (created.role === Roles.Employee) setScheduleEmployee(created); }}/>
            <WorkScheduleDialog employee={scheduleEmployee} onClose={() => setScheduleEmployee(null)}/>
            <UpdateEmployeeDialog open={openEditing} setOpen={setOpenEditing} editingEmployee={editingEmployee} setEditingEmployee={setEditingEmployee}/>
        </div>
    );
}

function UserTable({employees, setEditingEmployee, setOpenEditing, setScheduleEmployee}: {employees: EmployeeDto[], setEditingEmployee: (editingEmployee: EmployeeDto | null) => void, setOpenEditing: (openEditing: boolean) => void, setScheduleEmployee: (employee: EmployeeDto) => void}) {

    const handleDeactivate = (id: string) => {
        api.api.userDeactivateUser({id: id}).then(() => window.location.reload()).catch(err => console.log(err));
    }

    const handleActivate = (id: string) => {
        api.api.userActivateUser({id: id}).then(() => window.location.reload()).catch(err => console.log(err));
    }

    const statusStyles = {
        Active: "bg-success-bg text-success-fg",
        Inactive: "bg-destructive-bg text-destructive-fg",
    };

    return (
        <div className="border rounded-xl bg-card">
            <Table>
                <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                    <TableRow>
                        <TableHead>Employee</TableHead>
                        <TableHead>Email</TableHead>
                        <TableHead>Role</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead className="text-right">
                            <span className="sr-only">Actions</span>
                        </TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                    {employees.length === 0 && (
                        <TableRow>
                            <TableCell colSpan={5} className="text-center text-muted-foreground">No users match your search.</TableCell>
                        </TableRow>
                    )}
                    {[...employees].sort((a, b) => a.name!.localeCompare(b.name!)).map(e => (
                        <TableRow key={e.id}>
                            <TableCell>
                                <div className="flex items-center gap-3">
                                    <Avatar>
                                        <AvatarFallback className="bg-muted text-xs font-medium text-[#023459]">
                                            {initials(e.name)}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div className="flex flex-col">
                                        <span className="font-medium text-[#023459]">{e.name}</span>
                                    </div>
                                </div>
                            </TableCell>
                            <TableCell>{e.email}</TableCell>
                            <TableCell>{e.role}</TableCell>
                            <TableCell>
                                <Badge className={statusStyles[e.status!]}>{e.status}</Badge>
                            </TableCell>
                            <TableCell className="text-right">
                                <DropdownMenu>
                                    <DropdownMenuTrigger render={
                                                              <button type="button" aria-label={`Manage ${e.name}`}
                                                                      className="inline-flex size-9 items-center justify-center rounded-lg text-[#023459] hover:bg-muted">
                                                                 <MoreHorizontal className="size-5"/>
                                                             </button>
                                                         }/>
                                    <DropdownMenuContent className="w-45">
                                        <DropdownMenuItem onClick={() => {setEditingEmployee(e); setOpenEditing(true)}}>
                                            <Edit/> Edit Employee
                                        </DropdownMenuItem>
                                        {/* Accountant- og system owner-kontiene har intet skema og kan ikke deaktiveres */}
                                        {e.role === Roles.Employee && <>
                                            <DropdownMenuItem onClick={() => setScheduleEmployee(e)}>
                                                <CalendarClock/> Work schedule
                                            </DropdownMenuItem>
                                            <DropdownMenuSeparator />
                                            {e.status === Status.Active && <DropdownMenuItem onClick={() => handleDeactivate(e.id!)}>
                                                <Ban/> Deactivate Employee
                                            </DropdownMenuItem>}
                                            {e.status === Status.Inactive && <DropdownMenuItem onClick={() => handleActivate(e.id!)}>
                                                <Ban/> Activate Employee
                                            </DropdownMenuItem>}
                                        </>}
                                    </DropdownMenuContent>
                                </DropdownMenu>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </div>
    );
}

function CreateEmployeeDialog({open, setOpen, onCreated}: {open: boolean, setOpen: (open: boolean) => void, onCreated: (employee: EmployeeDto) => void}) {

    const [name, setName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [role, setRole] = useState<Roles>(Roles.Employee);
    const [startDate, setStartDate] = useState<string>(new Date().toLocaleDateString("sv-SE")); // YYYY-MM-DD i lokal tid

    const [submitting, setSubmitting] = useState<boolean>(false);
    const [error, setError] = useState<string | null>()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault(); // ellers genindlæser browseren siden, før kaldet er sendt
        setSubmitting(true);
        setError(null);
        try {
            const created = await api.api.userCreateUser({name, email, password, role, startDate});
            setOpen(false);
            setName("");
            setEmail("");
            setPassword("");
            onCreated(created);
        } catch (err: any) {
            // API'et sender { title: "..." } – læs den, hvis den er der
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.title ?? "Could not create the employee. Check the details and try again.");
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogContent className="sm:max-w-md border-2">
                <DialogHeader>
                    <DialogTitle>Add employee</DialogTitle>
                    <DialogDescription>Create an account and assign a role.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-2">
                        <Label htmlFor="name">Name</Label>
                        <Input id="name" value={name} onChange={e => setName(e.target.value)} required />
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="email">Email</Label>
                        <Input id="email" type="email" placeholder="name@businessesbjerg.com" value={email} onChange={e => setEmail(e.target.value)} required />
                    </div>
                    <div className="grid gap-2">
                        <Label>Role</Label>
                        <Select value={role} onValueChange={v => v && setRole(v as Roles)}>
                            <SelectTrigger><SelectValue /></SelectTrigger>
                            <SelectContent>
                                {Object.values(Roles).map(r => <SelectItem key={r} value={r}>{r}</SelectItem>)}
                            </SelectContent>
                        </Select>
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="startDate">Start date</Label>
                        <Input id="startDate" type="date" value={startDate} onChange={e => setStartDate(e.target.value)} required />
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="password">Temporary password</Label>
                        <Input id="password" type="password" minLength={8} value={password}
                               onChange={e => setPassword(e.target.value)} required />
                    </div>

                    {error && <p className="text-sm text-destructive">{error}</p>}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Cancel</Button>} />
                        <Button type="submit" disabled={submitting}>Create</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

function UpdateEmployeeDialog({open, setOpen, editingEmployee, setEditingEmployee}: {open: boolean, setOpen: (open: boolean) => void, editingEmployee: EmployeeDto | null, setEditingEmployee: (editingEmployee: EmployeeDto | null) => void}) {

    const [name, setName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [role, setRole] = useState<Roles | null>(null)

    const [submitting, setSubmitting] = useState<boolean>(false);
    const [error, setError] = useState<string | null>()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setError(null);
        try {
            await api.api.userUpdateUser({id: editingEmployee?.id!}, {name: name, email: email, role: role!});
            window.location.reload();
        } catch (err: any) {
            // Fx "There is already an accountant account."
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.title ?? "Could not save the changes. Try again.");
        } finally {
            setSubmitting(false);
        }
    }

    useEffect(() => {

        if (open) {
            setName(editingEmployee?.name!);
            setEmail(editingEmployee?.email!);
            setRole(editingEmployee?.role!);
        }
        else {
            setEditingEmployee(null);
        }

    }, [open]);

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogContent className="sm:max-w-md border-2">
                <DialogHeader>
                    <DialogTitle>Edit employee</DialogTitle>
                    <DialogDescription>Change user information and save changes.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <div className="grid gap-2">
                        <Label htmlFor="name">Name</Label>
                        <Input id="name" value={name} onChange={e => setName(e.target.value)} required />
                    </div>
                    <div className="grid gap-2">
                        <Label htmlFor="email">Email</Label>
                        <Input id="email" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
                    </div>
                    <div className="grid gap-2">
                        <Label>Role</Label>
                        <Select value={role} onValueChange={v => v && setRole(v as Roles)}>
                            <SelectTrigger><SelectValue /></SelectTrigger>
                            <SelectContent>
                                {Object.values(Roles).map(r => <SelectItem key={r} value={r}>{r}</SelectItem>)}
                            </SelectContent>
                        </Select>
                    </div>

                    {error && <p className="text-sm text-destructive">{error}</p>}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Cancel</Button>} />
                        <Button type="submit" disabled={submitting}>Save Changes</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}